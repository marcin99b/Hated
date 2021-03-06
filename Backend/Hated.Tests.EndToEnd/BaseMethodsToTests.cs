﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hated.Core.Domain;
using Hated.Infrastructure.Commands.Account;
using Hated.Infrastructure.Commands.Comment;
using Hated.Infrastructure.Commands.Posts;
using Hated.Infrastructure.Commands.Users;
using Hated.Infrastructure.DTO;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hated.Tests.EndToEnd
{
    public class BaseMethodsToTests : ServerConfigAndRun
    {
        protected UserDto testUserGenerate { get; private set; }
        protected string testEmailGenerate { get; private set; }
        protected string testPasswordGenerate { get; private set; }

        public BaseMethodsToTests()
        {
            Task.Run(ConstructorAsync).Wait();
        }

        private async Task ConstructorAsync()
        {
            testEmailGenerate = Guid.NewGuid() + "@email.com";
            testPasswordGenerate = "secret";
            await CreateNewUser(testEmailGenerate, null, testPasswordGenerate);
            await LoginUserAsync(testEmailGenerate, testPasswordGenerate);
            testUserGenerate = await GetUserAsync(testEmailGenerate);
        }

        protected StringContent GetPayload(object data)
        {
            var json = JsonConvert.SerializeObject(data);

            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        #region User
        //User
        protected async Task<HttpResponseMessage> CreateNewUser(string email, string username = null, string password = null)
        {
            var payload = GetPayload(new CreateUser
            {
                Email = email,
                Username = username ?? Guid.NewGuid().ToString().Substring(0, 30),
                Password = password ?? Guid.NewGuid().ToString()
            });
            
            return await Client.PostAsync("account/register", payload);
        }

        protected async Task<UserDto> GetUserAsync(string email)
        {
            var response = await Client.GetAsync($"users/{email}");
            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<UserDto>(responseString);
        }
        #endregion

        #region Account
        //Account
        protected async Task<JwtDto> GetTokenByLoginUserAsync(string email, string password)
        {
            var response = await Client.PostAsync("account/login", GetPayload(new LoginUser
            {
                Email = email,
                Password = password
            }));
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JwtDto>(responseString);
        }

        protected async Task LoginUserAsync(string email, string password)
        {
            var jwt = await GetTokenByLoginUserAsync(email, password);
            var token = jwt.Token;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        #endregion

        #region Post

        protected string GetRandomTextAsync(int? number = null)
        { 
            var texts = "";
            number = number ?? new Random().Next(0, 50);
            for (var i = 0; i < number; i++)
            {
                var content = RandomString(50);
                texts += "\n\n" + content;
            }
            return texts;
        }

        protected string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        //Post
        protected async Task<HttpResponseMessage> CreateNewPost(string content = null)
        {
            content = content ?? GetRandomTextAsync();
            var payload = GetPayload(new CreatePost
            {
                Title = GetRandomTextAsync(),
                Content = content
            });
            return await Client.PostAsync("posts", payload);
        }

        protected async Task<PostDto> GetPostAsync(string location)
        {
            var response = await Client.GetAsync(location);
            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PostDto>(responseString);
        }

        protected async Task<PostDto> CreateAndGetRandomPost(UserDto user)
        {
            var responsePost = await CreateNewPost();
            return await GetPostAsync(responsePost.Headers.Location.ToString());
        }
        #endregion

        #region Comment
        //Comment
        protected async Task<HttpResponseMessage> CreateNewComment(string postId, string content = null)
        {
            var payload = GetPayload(new CreateComment
            {
                PostId = postId,
                Content = content ?? GetRandomTextAsync()
            });
            return await Client.PostAsync("comments", payload);
        }

        protected async Task<CommentDto> GetCommentAsync(string location)
        {
            var response = await Client.GetAsync(location);
            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<CommentDto>(responseString);
        }

        protected async Task<CommentDto> CreateAndGetRandomComment(PostDto post, UserDto user)
        {
            var responseComment = await CreateNewComment(post.Id);
            return await GetCommentAsync(responseComment.Headers.Location.ToString());
        }

        #endregion
        
    }
}
