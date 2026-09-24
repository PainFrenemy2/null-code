import { useState } from 'react'
import { motion, useMotionValueEvent, useScroll } from 'framer-motion'
import Logo from './Logo.jsx'
import Scramble from './Scramble.jsx'
import { goRoute, goSection } from '../router.js'

// Совместимость со старыми импортами
export const scrollTo = goSection

const links = [
  { label: 'Проекты', route: '#/projects' },
  { label: 'Направления', section: '#services' },
  { label: 'Как мы работаем', section: '#process' },
  { label: 'Сообщество', section: '#contact' },
]

export default function Nav({ route }) {
  const { scrollY, scrollYProgress } = useScroll()
  const [hidden, setHidden] = useState(false)
  const [open, setOpen] = useState(false)

  useMotionValueEvent(scrollY, 'change', (v) => {
    const prev = scrollY.getPrevious() ?? 0
    setHidden(v > prev && v > 200 && !open)
  })

  return (
    <>
      <motion.div className="progress" style={{ scaleX: scrollYProgress }} />
      <motion.header className="nav" animate={{ y: hidden ? '-120%' : 0 }} transition={{ duration: 0.35, ease: 'easeOut' }}>
        <a href="#/" className="nav__brand" onClick={goRoute('#/')}>
          <Logo size={34} />
          <span className="mono">
            null<span className="dim">/</span>code
          </span>
        </a>
        <nav className={`nav__links ${open ? 'is-open' : ''}`}>
          {links.map((l, i) => {
            const active = l.route && route === 'projects'
            return (
              <a
                key={l.label}
                href={l.route || l.section}
                className={active ? 'is-active' : ''}
                onClick={(e) => {
                  setOpen(false)
                  if (l.route) goRoute(l.route)(e)
                  else goSection(l.section)(e)
                }}
              >
                <span className="mono dim">0{i + 1}</span>
                <Scramble text={l.label} hover />
              </a>
            )
          })}
        </nav>
        <a href="#/projects" className="btn btn--small nav__cta" onClick={goRoute('#/projects')}>
          Скачать ↓
        </a>
        <button className={`burger ${open ? 'is-open' : ''}`} onClick={() => setOpen(!open)} aria-label="Меню">
          <span />
          <span />
        </button>
      </motion.header>
    </>
  )
}
