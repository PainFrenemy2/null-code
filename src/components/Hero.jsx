import { useRef } from 'react'
import { motion, useScroll, useTransform } from 'framer-motion'
import CodeField from './CodeField.jsx'
import Logo from './Logo.jsx'
import Scramble from './Scramble.jsx'
import Magnetic from './Magnetic.jsx'
import { scrollTo } from './Nav.jsx'

const ease = [0.22, 1, 0.36, 1]

function Letters({ text, delay = 0, ready }) {
  return (
    <span className="letters">
      {text.split('').map((ch, i) => (
        <span className="letters__mask" key={i}>
          <motion.span
            className={ch === '/' ? 'slash' : ''}
            initial={{ y: '110%', rotate: 8 }}
            animate={ready ? { y: 0, rotate: 0 } : {}}
            transition={{ duration: 1, delay: delay + i * 0.05, ease }}
          >
            {ch}
          </motion.span>
        </span>
      ))}
    </span>
  )
}

export default function Hero({ ready }) {
  const ref = useRef(null)
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start start', 'end start'] })
  const y = useTransform(scrollYProgress, [0, 1], ['0%', '40%'])
  const scale = useTransform(scrollYProgress, [0, 1], [1, 0.85])
  const opacity = useTransform(scrollYProgress, [0, 0.8], [1, 0])
  const rot = useTransform(scrollYProgress, [0, 1], [0, 180])

  return (
    <section className="hero" id="top" ref={ref}>
      <CodeField />
      <div className="hero__vignette" />
      <motion.div className="hero__inner" style={{ y, scale, opacity }}>
        <motion.div
          className="hero__tag mono"
          initial={{ opacity: 0, y: 20 }}
          animate={ready ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.8, delay: 0.1 }}
        >
          <span className="pulse" /> студия разработки · открыты к проектам
        </motion.div>

        <h1 className="hero__title">
          <Letters text="NULL/" ready={ready} delay={0.15} />
          <Letters text="CODE" ready={ready} delay={0.45} />
        </h1>

        <div className="hero__sub">
          <p className="mono">
            <span className="dim">{'// '}</span>
            <Scramble text="из нуля — в работающий продукт" start={ready} />
          </p>
          <motion.p
            className="hero__lead"
            initial={{ opacity: 0 }}
            animate={ready ? { opacity: 1 } : {}}
            transition={{ duration: 1, delay: 1.1 }}
          >
            Сайты, веб-приложения, боты, мобильные приложения, игры, бэкенд и дизайн. Берём идею, которой ещё нет, и
            превращаем её в код.
          </motion.p>
        </div>

        <motion.div
          className="hero__actions"
          initial={{ opacity: 0, y: 20 }}
          animate={ready ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.8, delay: 1.3 }}
        >
          <Magnetic>
            <a href="#contact" className="btn" onClick={scrollTo('#contact')}>
              Начать проект <span className="arrow">→</span>
            </a>
          </Magnetic>
          <Magnetic>
            <a href="#services" className="btn btn--ghost" onClick={scrollTo('#services')}>
              Что мы делаем
            </a>
          </Magnetic>
        </motion.div>
      </motion.div>

      <motion.div
        className="hero__badge"
        initial={{ opacity: 0, scale: 0.6 }}
        animate={ready ? { opacity: 1, scale: 1 } : {}}
        transition={{ duration: 1, delay: 1.4, ease }}
      >
        <motion.svg viewBox="0 0 200 200" className="hero__ring" style={{ rotate: rot }}>
          <defs>
            <path id="circle" d="M100,100 m-80,0 a80,80 0 1,1 160,0 a80,80 0 1,1 -160,0" />
          </defs>
          <text textLength="500" lengthAdjust="spacing">
            <textPath href="#circle">NULL/CODE • FROM ZERO TO CODE • NULL/CODE • ZERO TO CODE • </textPath>
          </text>
        </motion.svg>
        <Logo size={64} />
      </motion.div>

      <motion.div className="hero__scroll mono" initial={{ opacity: 0 }} animate={ready ? { opacity: 1 } : {}} transition={{ delay: 1.8 }}>
        scroll
        <span className="hero__scroll-line" />
      </motion.div>
    </section>
  )
}
