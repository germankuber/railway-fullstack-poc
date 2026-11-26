import { API_URL } from '../config'
import { tokenStorage } from './tokenStorage'
import type { LoginResponse, Todo } from './types'

export class UnauthorizedError extends Error {
  constructor() {
    super('Your session has expired. Please sign in again.')
  }
}

export class ApiError extends Error {}

type UnauthorizedListener = () => void

let unauthorizedListener: UnauthorizedListener = () => {}

export const onUnauthorized = (listener: UnauthorizedListener): void => {
  unauthorizedListener = listener
}

const extractErrorMessage = async (response: Response): Promise<string> => {
  try {
    const body = await response.json()
    if (body?.errors) {
      return Object.values(body.errors).flat().join(' ')
    }
    return body?.error ?? body?.detail ?? body?.title ?? `Request failed (${response.status})`
  } catch {
    return `Request failed (${response.status})`
  }
}

const request = async <T>(path: string, init: RequestInit = {}, authenticated = true): Promise<T> => {
  const headers = new Headers(init.headers)
  if (init.body) {
    headers.set('Content-Type', 'application/json')
  }
  const token = tokenStorage.get()
  if (authenticated && token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_URL}${path}`, { ...init, headers })

  if (response.status === 401 && authenticated) {
    unauthorizedListener()
    throw new UnauthorizedError()
  }
  if (!response.ok) {
    throw new ApiError(await extractErrorMessage(response))
  }
  const body = await response.text()
  return (body ? JSON.parse(body) : undefined) as T
}

const jsonBody = (payload: unknown): string => JSON.stringify(payload)

export const authApi = {
  register: (email: string, password: string) =>
    request<void>('/auth/register', { method: 'POST', body: jsonBody({ email, password }) }, false),
  login: (email: string, password: string) =>
    request<LoginResponse>('/auth/login', { method: 'POST', body: jsonBody({ email, password }) }, false),
}

export const todosApi = {
  list: () => request<Todo[]>('/api/todos'),
  create: (title: string) => request<Todo>('/api/todos', { method: 'POST', body: jsonBody({ title }) }),
  setDone: (id: string, isDone: boolean) =>
    request<Todo>(`/api/todos/${id}`, { method: 'PATCH', body: jsonBody({ isDone }) }),
  remove: (id: string) => request<void>(`/api/todos/${id}`, { method: 'DELETE' }),
}
