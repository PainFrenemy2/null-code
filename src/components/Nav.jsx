import { useState } from 'react'
import { motion, useMotionValueEvent, useScroll } from 'framer-motion'
import Logo from './Logo.jsx'
import Scramble from './Scramble.jsx'

export const scrollTo = (id) => (e) => {
  e.preventDefault()
  const el = document.querySelector(id)
  if (!el) return
  if (window.__lenis) window.__lenis.scrollTo(el, { offset: -20 })
  else el.scrollIntoView({ behavior: 'smooth' })
}

const links = [
  ['#services', 'Услуги'],
  ['#process', 'Процесс'],
  ['#stack', 'Стек'],
  ['#contact', 'Контакты'],
]

export default function Nav() {
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
        <a href="#top" className="nav__brand" onClick={scrollTo('#top')}>
          <Logo size={34} />
          <span className="mono">
            null<span className="dim">/</span>code
          </span>
        </a>
        <nav className={`nav__links ${open ? 'is-open' : ''}`}>
          {links.map(([href, label], i) => (
            <a
              key={href}
              href={href}
              onClick={(e) => {
                setOpen(false)
                scrollTo(href)(e)
              }}
            >
              <span className="mono dim">0{i + 1}</span>
              <Scramble text={label} hover />
            </a>
          ))}
        </nav>
        <a href="#contact" className="btn btn--small nav__cta" onClick={scrollTo('#contact')}>
          Обсудить проект
        </a>
        <button className={`burger ${open ? 'is-open' : ''}`} onClick={() => setOpen(!open)} aria-label="Меню">
          <span />
          <span />
        </button>
      </motion.header>
    </>
  )
}
