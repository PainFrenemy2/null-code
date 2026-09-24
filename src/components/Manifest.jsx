import { useRef } from 'react'
import { motion, useScroll, useTransform } from 'framer-motion'

const text =
  'Нам не нужны клиенты и дедлайны. Мы делаем то, что интересно нам самим: мониторинг, лаунчеры, моды, ботов, утилиты. Каждый проект начинается с null, а заканчивается релизом, который может скачать любой.'

function Word({ children, progress, range }) {
  const opacity = useTransform(progress, range, [0.12, 1])
  return (
    <span className="word">
      <motion.span style={{ opacity }}>{children}</motion.span>{' '}
    </span>
  )
}

export default function Manifest() {
  const ref = useRef(null)
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start 0.85', 'end 0.4'] })
  const words = text.split(' ')

  return (
    <section className="manifest section" ref={ref}>
      <div className="label mono">[ 00 ] манифест</div>
      <p className="manifest__text">
        {words.map((w, i) => (
          <Word key={i} progress={scrollYProgress} range={[i / words.length, (i + 1) / words.length]}>
            {w}
          </Word>
        ))}
      </p>
    </section>
  )
}
