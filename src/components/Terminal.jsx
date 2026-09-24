import { useEffect, useRef, useState } from 'react'
import { motion, useInView } from 'framer-motion'
import Reveal from './Reveal.jsx'

const script = [
  { t: 'cmd', v: 'git commit -m "monitoring: first release"' },
  { t: 'out', v: '✔ сборка windows-x64 ........ done' },
  { t: 'out', v: '✔ сборка linux-x64 .......... done' },
  { t: 'out', v: '✔ сборка macos-arm64 ........ done' },
  { t: 'out', v: '✔ тесты ..................... passed' },
  { t: 'cmd', v: 'gh release create v1.0.0 ./dist/*' },
  { t: 'out', v: '✔ 3 файла загружено в релиз' },
  { t: 'ok', v: '↓ доступно для скачивания → null/code / проекты' },
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
        <div className="label mono">[ 04 ] от коммита до загрузки</div>
        <Reveal as="h2" className="h2">
          Один релиз —<br />
          <span className="outline-text">и все качают</span>
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
