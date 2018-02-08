﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Hated.Core.Domain
{
    public class Post
    {
        private readonly ISet<Comment> _comments = new HashSet<Comment>();

        public Guid Id { get; protected set; }
        public Guid UserId { get; protected set; }
        public string Content { get; protected set; }
        public IEnumerable<Comment> Comments => _comments;
        public DateTime ChangedAt { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        public Post(Guid userId, string content)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            SetContent(content);
            CreatedAt = DateTime.UtcNow;
        }

        public void SetContent(string content)
        {
            Content = content;
            ChangedAt = DateTime.UtcNow;
        }

        public void AddComment(Comment comment)
        {
            _comments.Add(comment);
            CreatedAt = DateTime.UtcNow;
        }

        public Comment GetComment(Guid commentId)
            => Comments.SingleOrDefault(x => x.Id == commentId);


        public void UpdateComment(Comment comment)
        {
            _comments.Where(x => x.Id == comment.Id).Select(x => comment).ToHashSet();
            CreatedAt = DateTime.UtcNow;
        }
        
        public void DeleteComment(Comment comment)
        {
            _comments.Remove(comment);

            CreatedAt = DateTime.UtcNow;
        }
    }
}