import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { authApi, onUnauthorized } from '../api/client'
import { tokenStorage } from '../api/tokenStorage'

interface AuthContextValue {
  isAuthenticated: boolean
  signIn: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  signOut: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => tokenStorage.get())

  const signOut = useCallback(() => {
    tokenStorage.clear()
    setToken(null)
  }, [])

  const signIn = useCallback(async (email: string, password: string) => {
    const { accessToken } = await authApi.login(email, password)
    tokenStorage.set(accessToken)
    setToken(accessToken)
  }, [])

  const register = useCallback(
    async (email: string, password: string) => {
      await authApi.register(email, password)
      await signIn(email, password)
    },
    [signIn],
  )

  useEffect(() => {
    onUnauthorized(signOut)
  }, [signOut])

  const value = useMemo(
    () => ({ isAuthenticated: token !== null, signIn, register, signOut }),
    [token, signIn, register, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }
  return context
}
