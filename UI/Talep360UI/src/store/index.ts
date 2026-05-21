import { createStore } from 'vuex';
import auth from './modules/auth';

// Vuex 4 Store
export default createStore({
    modules: {
        auth
    }
});