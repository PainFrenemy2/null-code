import { useState } from 'react'
import { motion } from 'framer-motion'
import Magnetic from './Magnetic.jsx'
import Reveal from './Reveal.jsx'

export const VK_URL = 'https://vk.ru/null.code'
const kinds = ['Сайт', 'Приложение', 'Бот', 'Игра', 'Дизайн', 'Бэкенд', 'Другое']

export default function Contact() {
  const [picked, setPicked] = useState(['Сайт'])
  const toggle = (k) => setPicked((p) => (p.includes(k) ? p.filter((x) => x !== k) : [...p, k]))

  return (
    <section className="section contact" id="contact">
      <div className="label mono">[ 05 ] контакты</div>
      <h2 className="contact__title">
        <Reveal as="span" className="line">Есть идея?</Reveal>
        <Reveal as="span" className="line outline-text" delay={0.1}>
          Превратим её
        </Reveal>
        <Reveal as="span" className="line" delay={0.2}>
          в <span className="mono glitch" data-text="code">code</span>
        </Reveal>
      </h2>

      <Reveal className="brief" delay={0.2}>
        <div className="mono dim">Что нужно сделать?</div>
        <div className="brief__chips">
          {kinds.map((k) => (
            <motion.button
              key={k}
              type="button"
              className={`chip mono ${picked.includes(k) ? 'is-on' : ''}`}
              onClick={() => toggle(k)}
              whileTap={{ scale: 0.92 }}
            >
              {picked.includes(k) ? '✓ ' : '+ '}
              {k}
            </motion.button>
          ))}
        </div>
        <Magnetic strength={0.25}>
          <a className="btn btn--big" href={VK_URL} target="_blank" rel="noreferrer">
            Написать в VK <span className="arrow">↗</span>
          </a>
        </Magnetic>
        <div className="mono dim brief__hint">
          {picked.length ? `// выбрано: ${picked.join(', ').toLowerCase()} — просто напишите это в сообщения группы` : '// выберите, что нужно'}
        </div>
      </Reveal>
    </section>
  )
}
