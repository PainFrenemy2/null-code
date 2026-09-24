import { useRef } from 'react'
import { motion, useScroll, useTransform } from 'framer-motion'

const steps = [
  ['null', 'Идея', 'Слушаем, задаём неудобные вопросы и понимаем, что вам на самом деле нужно.'],
  ['plan', 'Проектирование', 'Архитектура, стек, сроки и прототип. Без сюрпризов в середине пути.'],
  ['design', 'Дизайн', 'Интерфейс, который приятно трогать, и стиль, который запоминается.'],
  ['code', 'Разработка', 'Пишем чистый код короткими итерациями и показываем прогресс каждую неделю.'],
  ['test', 'Тесты', 'Ломаем всё сами, чтобы не сломали пользователи.'],
  ['deploy', 'Запуск', 'Релиз, мониторинг и поддержка. Продукт живёт — мы рядом.'],
]

export default function Process() {
  const ref = useRef(null)
  const { scrollYProgress } = useScroll({ target: ref })
  const x = useTransform(scrollYProgress, [0, 1], ['0%', '-72%'])
  const bar = useTransform(scrollYProgress, [0, 1], [0, 1])

  return (
    <section className="process" id="process" ref={ref}>
      <div className="process__sticky">
        <div className="process__head">
          <div className="label mono">[ 02 ] процесс</div>
          <h2 className="h2">
            От <span className="mono">null</span> до релиза
          </h2>
          <div className="process__bar">
            <motion.div style={{ scaleX: bar }} />
          </div>
        </div>
        <motion.div className="process__track" style={{ x }}>
          {steps.map(([cmd, title, text], i) => (
            <div className="step" key={cmd}>
              <div className="step__num">0{i + 1}</div>
              <div className="step__cmd mono">
                <span className="dim">$</span> nc {cmd}
              </div>
              <h3>{title}</h3>
              <p>{text}</p>
            </div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}
