import { useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'

type Mode = 'login' | 'register'

export function AuthForm() {
  const { signIn, register } = useAuth()
  const [mode, setMode] = useState<Mode>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await (mode === 'login' ? signIn(email, password) : register(email, password))
    } catch (failure) {
      setError(failure instanceof Error ? failure.message : 'Something went wrong')
    } finally {
      setBusy(false)
    }
  }

  const toggleMode = () => {
    setMode(mode === 'login' ? 'register' : 'login')
    setError(null)
  }

  return (
    <form className="card" onSubmit={submit}>
      <h1>{mode === 'login' ? 'Sign in' : 'Create account'}</h1>
      <label>
        Email
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="email" />
      </label>
      <label>
        Password
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          minLength={6}
          autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
        />
      </label>
      {error && <p className="error">{error}</p>}
      <button type="submit" className="primary" disabled={busy}>
        {mode === 'login' ? 'Sign in' : 'Register'}
      </button>
      <button type="button" className="link" onClick={toggleMode}>
        {mode === 'login' ? 'Need an account? Register' : 'Already registered? Sign in'}
      </button>
    </form>
  )
}
