import { useEffect, useRef, useState } from 'react'
import { motion, useInView } from 'framer-motion'
import Reveal from './Reveal.jsx'

const script = [
  { t: 'cmd', v: 'npx create-null-code your-idea' },
  { t: 'out', v: '✔ анализ задачи ............ done' },
  { t: 'out', v: '✔ подбор стека ............. react · node · postgres' },
  { t: 'out', v: '✔ дизайн ................... figma → ui' },
  { t: 'out', v: '✔ разработка ............... 100%' },
  { t: 'out', v: '✔ тесты .................... passed' },
  { t: 'cmd', v: 'nc deploy --prod' },
  { t: 'ok', v: '🚀 your-idea is live → https://your-idea.app' },
]

export default function Terminal() {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-100px' })
  const [lines, setLines] = useState([])
  const [typing, setTyping] = useState('')

  useEffect(() => {
    if (!inView) return
    let cancelled = false
    const sleep = (ms) => new Promise((r) => setTimeout(r, ms))
    ;(async () => {
      for (const line of script) {
        if (cancelled) return
        if (line.t === 'cmd') {
          for (let i = 1; i <= line.v.length; i++) {
            if (cancelled) return
            setTyping(line.v.slice(0, i))
            await sleep(35 + Math.random() * 40)
          }
          await sleep(300)
          setTyping('')
        } else await sleep(380)
        setLines((l) => [...l, line])
      }
    })()
    return () => {
      cancelled = true
    }
  }, [inView])

  const done = lines.length === script.length

  return (
    <section className="section terminal-section">
      <div className="section__head">
        <div className="label mono">[ 03 ] как это выглядит</div>
        <Reveal as="h2" className="h2">
          Одна команда —<br />
          <span className="outline-text">и идея живая</span>
        </Reveal>
      </div>
      <Reveal>
        <div className="terminal" ref={ref}>
          <div className="terminal__bar">
            <i />
            <i />
            <i />
            <span className="mono">~/null-code — zsh</span>
          </div>
          <div className="terminal__body mono">
            {lines.map((l, i) => (
              <motion.div key={i} className={`tl tl--${l.t}`} initial={{ opacity: 0, x: -10 }} animate={{ opacity: 1, x: 0 }}>
                {l.t === 'cmd' && <span className="prompt">null@code:~$ </span>}
                {l.v}
              </motion.div>
            ))}
            {!done && (
              <div className="tl tl--cmd">
                <span className="prompt">null@code:~$ </span>
                {typing}
                <span className="caret" />
              </div>
            )}
            {done && (
              <div className="tl tl--cmd">
                <span className="prompt">null@code:~$ </span>
                <span className="caret" />
              </div>
            )}
          </div>
        </div>
      </Reveal>
    </section>
  )
}
