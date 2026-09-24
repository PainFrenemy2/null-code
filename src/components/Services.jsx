import { useRef } from 'react'
import { motion, useMotionValue, useSpring, useMotionTemplate } from 'framer-motion'
import Reveal from './Reveal.jsx'
import Scramble from './Scramble.jsx'

const services = [
  { tag: 'web', title: 'Сайты и веб-приложения', text: 'Лендинги, магазины, SPA, личные кабинеты и админки. React, Next.js, Vue.', icon: '</>' },
  { tag: 'mobile', title: 'Мобильные приложения', text: 'iOS и Android на React Native и Flutter — один код, две платформы.', icon: '[ ]' },
  { tag: 'bots', title: 'Боты и автоматизация', text: 'Telegram и VK боты, парсеры, интеграции и скрипты, которые экономят часы.', icon: '>_' },
  { tag: 'backend', title: 'Бэкенд и API', text: 'Node.js, Python, Go. Базы данных, очереди, микросервисы, высокая нагрузка.', icon: '{ }' },
  { tag: 'gamedev', title: 'Игры', text: 'Браузерные и мобильные игры, Unity, моды и сервера для любимых проектов.', icon: '▲▼' },
  { tag: 'design', title: 'UI/UX и брендинг', text: 'Интерфейсы, прототипы в Figma, логотипы и айдентика, как у нас самих.', icon: '◐' },
  { tag: 'ai', title: 'AI-интеграции', text: 'Нейросети в вашем продукте: чат-боты, генерация, анализ данных.', icon: '∞' },
  { tag: 'devops', title: 'DevOps и поддержка', text: 'Деплой, CI/CD, Docker, серверы, мониторинг. Не бросаем после релиза.', icon: '⟳' },
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
        <div className="label mono">[ 01 ] услуги</div>
        <Reveal as="h2" className="h2">
          Делаем <span className="outline-text">всё</span>,<br />что пишется кодом
        </Reveal>
        <Reveal className="section__note mono" delay={0.2}>
          <Scramble text="full-stack · full-cycle · full-power" hover />
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
