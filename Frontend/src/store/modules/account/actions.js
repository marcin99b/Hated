import Endpoints from '@/services/Endpoints';

export default {
  logIn({ commit }, user) {
    Endpoints.logIn(user)
      .then(({ data }) => {
        commit('logIn', data);
      });
  },
  logOut({ commit }) {
    commit('logOut');
  },
  checkIsAlreadyLogged({ commit }) {
    commit('checkIsAlreadyLogged');
  },
  clearError({ commit }) {
    commit('clearError');
  }
};