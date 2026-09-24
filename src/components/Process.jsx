import { useEffect, useRef, useState } from 'react'
import { motion, useScroll, useTransform } from 'framer-motion'

const steps = [
  ['null', 'Идея', 'Кто-то в сообществе говорит «а было бы круто…». С этого всё и начинается.'],
  ['code', 'Код', 'Пишем вместе, открыто, на GitHub. Каждый может заглянуть и помочь.'],
  ['test', 'Тесты', 'Гоняем на своих машинах и ломаем сами, чтобы не сломалось у вас.'],
  ['build', 'Сборка', 'Компилируем под Windows, macOS и Linux. Готовые файлы, без танцев с бубном.'],
  ['release', 'Релиз', 'Выкладываем в GitHub Releases. Сборка сразу появляется во вкладке «Проекты».'],
  ['update', 'Обновления', 'Слушаем фидбек в VK, чиним баги, добавляем фичи. Проект живёт.'],
]

export default function Process() {
  const ref = useRef(null)
  const track = useRef(null)
  const [dist, setDist] = useState(0)
  const { scrollYProgress } = useScroll({ target: ref })
  const x = useTransform(scrollYProgress, [0, 1], [0, -dist])

  useEffect(() => {
    const measure = () => setDist(Math.max(0, track.current.scrollWidth - window.innerWidth))
    measure()
    window.addEventListener('resize', measure)
    return () => window.removeEventListener('resize', measure)
  }, [])
  const bar = useTransform(scrollYProgress, [0, 1], [0, 1])

  return (
    <section className="process" id="process" ref={ref}>
      <div className="process__sticky">
        <div className="process__head">
          <div className="label mono">[ 03 ] как мы работаем</div>
          <h2 className="h2">
            От <span className="mono">null</span> до релиза
          </h2>
          <div className="process__bar">
            <motion.div style={{ scaleX: bar }} />
          </div>
        </div>
        <motion.div className="process__track" ref={track} style={{ x }}>
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
