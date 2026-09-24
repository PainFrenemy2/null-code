import { motion } from 'framer-motion'
import Reveal from './Reveal.jsx'

const stack = [
  'React', 'Next.js', 'Vue', 'TypeScript', 'Node.js', 'Python', 'Go', 'PHP', 'Laravel', 'Django',
  'PostgreSQL', 'MongoDB', 'Redis', 'Docker', 'Linux', 'Nginx', 'React Native', 'Flutter', 'Kotlin', 'Swift',
  'Unity', 'C#', 'C++', 'Lua', 'Three.js', 'Figma', 'Blender', 'Telegram API', 'VK API', 'OpenAI',
]

export default function Stack() {
  return (
    <section className="section" id="stack">
      <div className="section__head">
        <div className="label mono">[ 04 ] стек</div>
        <Reveal as="h2" className="h2">
          Инструменты <span className="outline-text">под задачу</span>,<br />а не задача под инструмент
        </Reveal>
      </div>
      <div className="stack">
        {stack.map((s, i) => (
          <motion.span
            key={s}
            className="chip mono"
            data-hover
            initial={{ opacity: 0, scale: 0.6, y: 20 }}
            whileInView={{ opacity: 1, scale: 1, y: 0 }}
            viewport={{ once: true, margin: '-60px' }}
            transition={{ type: 'spring', stiffness: 260, damping: 18, delay: i * 0.025 }}
            whileHover={{ scale: 1.12, rotate: i % 2 ? 3 : -3 }}
          >
            {s}
          </motion.span>
        ))}
      </div>
      <div className="stats">
        {[
          ['0', 'готовых шаблонов — всё с нуля'],
          ['∞', 'направлений разработки'],
          ['24/7', 'на связи в VK'],
          ['1', 'команда на весь цикл'],
        ].map(([n, t], i) => (
          <Reveal key={t} delay={i * 0.1} className="stat">
            <div className="stat__n">{n}</div>
            <div className="stat__t mono">{t}</div>
          </Reveal>
        ))}
      </div>
    </section>
  )
}
