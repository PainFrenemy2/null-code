import { useRef } from 'react'
import { motion, useMotionValue, useSpring, useMotionTemplate } from 'framer-motion'
import Reveal from './Reveal.jsx'
import Scramble from './Scramble.jsx'

const services = [
  { tag: 'desktop', title: 'Программы и утилиты', text: 'Десктопные приложения под Windows, macOS и Linux: скачал, запустил, пользуешься.', icon: '[_]' },
  { tag: 'monitoring', title: 'Мониторинг', text: 'Следим за системой, серверами и сервисами. Графики, алерты, всё в одном окне.', icon: '▁▃▆' },
  { tag: 'mods', title: 'Моды и лаунчеры', text: 'Моды, сборки и лаунчеры для игр и серверов, которые мы сами любим.', icon: '▲▼' },
  { tag: 'bots', title: 'Боты и автоматизация', text: 'Боты для VK и Telegram, парсеры и скрипты, которые экономят часы.', icon: '>_' },
  { tag: 'web', title: 'Веб', text: 'Сайты, панели и веб-сервисы. Как этот сайт, например.', icon: '</>' },
  { tag: 'backend', title: 'Бэкенд и API', text: 'Серверы, базы данных и API, на которых держатся наши проекты.', icon: '{ }' },
  { tag: 'gamedev', title: 'Игры', text: 'Небольшие игры и эксперименты: от браузерных до движковых.', icon: '◆' },
  { tag: 'design', title: 'Дизайн', text: 'Интерфейсы, логотипы и айдентика. Всё, чтобы проекты выглядели так же хорошо, как работают.', icon: '◐' },
]

function Card({ s, i }) {
  const ref = useRef(null)
  const mx = useMotionValue(-200)
  const my = useMotionValue(-200)
  const rx = useSpring(useMotionValue(0), { stiffness: 150, damping: 15 })
  const ry = useSpring(useMotionValue(0), { stiffness: 150, damping: 15 })
  const bg = useMotionTemplate`radial-gradient(320px circle at ${mx}px ${my}px, rgba(255,255,255,0.12), transparent 70%)`

  const move = (e) => {
    const r = ref.current.getBoundingClientRect()
    const x = e.clientX - r.left
    const y = e.clientY - r.top
    mx.set(x)
    my.set(y)
    rx.set(((y / r.height) - 0.5) * -10)
    ry.set(((x / r.width) - 0.5) * 10)
  }
  const leave = () => {
    rx.set(0)
    ry.set(0)
    mx.set(-200)
    my.set(-200)
  }

  return (
    <Reveal delay={(i % 4) * 0.08}>
      <motion.article
        ref={ref}
        className="card"
        data-hover
        onPointerMove={move}
        onPointerLeave={leave}
        style={{ rotateX: rx, rotateY: ry }}
      >
        <motion.div className="card__glow" style={{ background: bg }} />
        <div className="card__top mono">
          <span className="dim">0{i + 1}</span>
          <span className="card__tag">#{s.tag}</span>
        </div>
        <div className="card__icon mono">{s.icon}</div>
        <h3>{s.title}</h3>
        <p>{s.text}</p>
      </motion.article>
    </Reveal>
  )
}

export default function Services() {
  return (
    <section className="section" id="services">
      <div className="section__head">
        <div className="label mono">[ 01 ] направления</div>
        <Reveal as="h2" className="h2">
          Пишем <span className="outline-text">всё</span>,<br />что интересно
        </Reveal>
        <Reveal className="section__note mono" delay={0.2}>
          <Scramble text="open source · бесплатно · для своих" hover />
        </Reveal>
      </div>
      <div className="grid">
        {services.map((s, i) => (
          <Card key={s.tag} s={s} i={i} />
        ))}
      </div>
    </section>
  )
}
