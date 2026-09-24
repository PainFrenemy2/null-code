import { useEffect, useState } from 'react'

// Хэш-роутинг: работает на любом статическом хостинге, включая GitHub Pages
const parse = () => (location.hash.startsWith('#/projects') ? 'projects' : 'home')

export function useRoute() {
  const [route, setRoute] = useState(parse)
  useEffect(() => {
    const on = () => setRoute(parse())
    window.addEventListener('hashchange', on)
    return () => window.removeEventListener('hashchange', on)
  }, [])
  return route
}

export function smoothTo(target, immediate = false) {
  const el = typeof target === 'string' ? document.querySelector(target) : target
  if (!el && target !== 0) return
  if (window.__lenis) window.__lenis.scrollTo(el ?? 0, { offset: -20, immediate })
  else if (el) el.scrollIntoView({ behavior: immediate ? 'auto' : 'smooth' })
  else window.scrollTo(0, 0)
}

// Переход к секции главной — с любой страницы
export const goSection = (id) => (e) => {
  e?.preventDefault()
  if (parse() !== 'home') {
    window.__pendingSection = id
    location.hash = '#/'
  } else smoothTo(id)
}

export const goRoute = (path) => (e) => {
  e?.preventDefault()
  if (location.hash === path) return smoothTo(0)
  location.hash = path
}
