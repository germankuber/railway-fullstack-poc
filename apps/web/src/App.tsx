import { AuthProvider, useAuth } from './auth/AuthContext'
import { AuthForm } from './components/AuthForm'
import { TodoScreen } from './components/TodoScreen'

function Router() {
  const { isAuthenticated } = useAuth()
  return isAuthenticated ? <TodoScreen /> : <AuthForm />
}

export default function App() {
  return (
    <AuthProvider>
      <main className="page">
        <Router />
      </main>
    </AuthProvider>
  )
}
