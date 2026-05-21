import { createRouter, createWebHistory } from 'vue-router';
import store from '@/store';
import HomeView from '../views/HomeView.vue';
import LoginView from '../views/LoginView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { layout: 'AuthLayout', guest: true }
    },
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: { layout: 'MainLayout', requiresAuth: true }
    },
    // Ticket Routes
    {
      path: '/dashboard/staff',
      name: 'staff-dashboard',
      component: () => import('../views/Dashboards/StaffDashboardView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true, roles: ['Staff', 'User'] }
    },
    {
      path: '/dashboard/manager',
      name: 'manager-dashboard',
      component: () => import('../views/Dashboards/ManagerDashboardView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true, roles: ['Manager', 'Admin', 'SuperAdmin'] }
    },
    {
      path: '/tickets/my-tickets',
      name: 'my-tickets',
      component: () => import('../views/Tickets/MyTicketsView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true }
    },
    {
      path: '/tickets/create',
      name: 'create-ticket',
      component: () => import('../views/Tickets/CreateTicketView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true }
    },
    {
      path: '/tickets/edit/:id',
      name: 'edit-ticket',
      component: () => import('../views/Tickets/EditTicketView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true }
    },
    // Tenant Admin Routes
    {
      path: '/users',
      name: 'users',
      component: () => import('../views/Admin/UsersView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true, roles: ['Admin'] }
    },
    // Admin Routes
    {
      path: '/admin/tenants',
      name: 'tenants',
      component: () => import('../views/Admin/TenantsView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true, roles: ['SuperAdmin'] }
    },
    {
      path: '/admin/tenants/:id/users',
      name: 'tenant-users',
      component: () => import('../views/Admin/TenantUsersView.vue'),
      meta: { layout: 'MainLayout', requiresAuth: true, roles: ['SuperAdmin'] }
    },
    // Catch-all redirect to Home
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
});

// Navigation Guard
router.beforeEach((to, from, next) => {
  const isAuthenticated = store.getters['auth/isAuthenticated'];
  const userRoles = store.getters['auth/getUserRoles'];

  // 1. Login gerektiren sayfa kontrolü
  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!isAuthenticated) {
      next('/login');
      return;
    }
  }

  // 2. Misafir (Guest) kontrolü (Login sayfasına giriş yapmış kullanıcı girmemeli)
  if (to.matched.some(record => record.meta.guest)) {
    if (isAuthenticated) {
      next('/');
      return;
    }
  }

  // 3. Rol kontrolü
  if (to.meta.roles) {
    const requiredRoles = to.meta.roles as string[];
    const hasRole = userRoles.some((role: string) => requiredRoles.includes(role));
    
    if (!hasRole) {
      // Yetkisiz erişim
      next('/'); // Veya 403 sayfasına yönlendir
      return;
    }
  }

  next();
});

export default router;