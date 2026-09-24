import { motion } from 'framer-motion'
import Magnetic from './Magnetic.jsx'
import Reveal from './Reveal.jsx'
import { GITHUB_URL, VK_URL } from '../config.js'

const ways = [
  ['↓', 'Качай', 'Пользуйся нашими проектами бесплатно'],
  ['★', 'Ставь звёзды', 'На GitHub. Нам приятно, а проектам полезно'],
  ['!', 'Пиши баги', 'В issues или в сообщения группы'],
  ['</>', 'Коди с нами', 'Pull request-ы и идеи приветствуются'],
]

export default function Contact() {
  return (
    <section className="section contact" id="contact">
      <div className="label mono">[ 06 ] сообщество</div>
      <h2 className="contact__title">
        <Reveal as="span" className="line">Есть идея</Reveal>
        <Reveal as="span" className="line outline-text" delay={0.1}>
          или руки,
        </Reveal>
        <Reveal as="span" className="line" delay={0.2}>
          что пишут <span className="mono glitch" data-text="code">code</span>?
        </Reveal>
      </h2>

      <div className="ways">
        {ways.map(([icon, title, text], i) => (
          <motion.div
            key={title}
            className="way"
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true, margin: '-60px' }}
            transition={{ duration: 0.7, delay: i * 0.08, ease: [0.22, 1, 0.36, 1] }}
          >
            <span className="way__icon mono">{icon}</span>
            <b>{title}</b>
            <span className="dim">{text}</span>
          </motion.div>
        ))}
      </div>

      <Reveal className="contact__actions" delay={0.2}>
        <Magnetic strength={0.25}>
          <a className="btn btn--big" href={VK_URL} target="_blank" rel="noreferrer">
            Вступить в VK <span className="arrow">↗</span>
          </a>
        </Magnetic>
        <Magnetic strength={0.25}>
          <a className="btn btn--big btn--ghost" href={GITHUB_URL} target="_blank" rel="noreferrer">
            GitHub <span className="arrow">↗</span>
          </a>
        </Magnetic>
      </Reveal>
    </section>
  )
}
