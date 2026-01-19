import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import api from '../services/api'

export const useAuthStore = create(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,

      login: async (username, password) => {
        try {
          const response = await api.post('/auth/login', { username, password })
          const { token } = response.data
          
          // Get user info
          api.defaults.headers.common['Authorization'] = `Bearer ${token}`
          const userResponse = await api.get('/auth/me')
          
          set({
            token,
            user: userResponse.data,
            isAuthenticated: true,
          })
          
          return { success: true }
        } catch (error) {
          return {
            success: false,
            message: error.response?.data?.message || 'Login failed',
          }
        }
      },

      logout: () => {
        delete api.defaults.headers.common['Authorization']
        set({
          token: null,
          user: null,
          isAuthenticated: false,
        })
      },

      setToken: (token) => {
        api.defaults.headers.common['Authorization'] = `Bearer ${token}`
        set({ token, isAuthenticated: !!token })
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ token: state.token, user: state.user }),
    }
  )
)

// Initialize auth header if token exists
const token = useAuthStore.getState().token
if (token) {
  api.defaults.headers.common['Authorization'] = `Bearer ${token}`
  useAuthStore.setState({ isAuthenticated: true })
}

