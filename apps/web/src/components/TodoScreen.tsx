import { useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'
import { useTodos } from '../todos/useTodos'
import { ConnectionBadge } from './ConnectionBadge'
import { TodoItemRow } from './TodoItemRow'

export function TodoScreen() {
  const { signOut } = useAuth()
  const { todos, status, error, add, toggle, remove } = useTodos()
  const [title, setTitle] = useState('')

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    const trimmed = title.trim()
    if (!trimmed) {
      return
    }
    setTitle('')
    await add(trimmed)
  }

  return (
    <div className="card">
      <header className="card-header">
        <h1>My todos</h1>
        <div className="card-actions">
          <ConnectionBadge status={status} />
          <button type="button" className="link" onClick={signOut}>
            Log out
          </button>
        </div>
      </header>
      <form className="add-form" onSubmit={submit}>
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="What needs to be done?"
          maxLength={200}
        />
        <button type="submit" className="primary">
          Add
        </button>
      </form>
      {error && <p className="error">{error}</p>}
      {todos.length === 0 ? (
        <p className="empty">Nothing to do yet.</p>
      ) : (
        <ul className="todo-list">
          {todos.map((todo) => (
            <TodoItemRow key={todo.id} todo={todo} onToggle={toggle} onDelete={remove} />
          ))}
        </ul>
      )}
    </div>
  )
}
