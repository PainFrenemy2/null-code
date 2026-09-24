import { useEffect, useRef, useState } from 'react'

const CHARS = '01/<>{}[]#$%&*+=?!_-\\|NULLCODE'

// Текст «расшифровывается» из мусорных символов
export default function Scramble({ text, start = true, speed = 28, className = '', hover = false }) {
  const [out, setOut] = useState(start ? '' : text.replace(/\S/g, ' '))
  const frame = useRef(0)
  const timer = useRef()

  const run = () => {
    clearInterval(timer.current)
    frame.current = 0
    const total = text.length * 3
    timer.current = setInterval(() => {
      const f = frame.current++
      const res = text
        .split('')
        .map((ch, i) => {
          if (ch === ' ') return ' '
          if (i * 3 < f) return ch
          return CHARS[Math.floor(Math.random() * CHARS.length)]
        })
        .join('')
      setOut(res)
      if (f >= total) {
        clearInterval(timer.current)
        setOut(text)
      }
    }, speed)
  }

  useEffect(() => {
    if (start) run()
    return () => clearInterval(timer.current)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [start, text])

  return (
    <span className={className} onMouseEnter={hover ? run : undefined} aria-label={text}>
      {out}
    </span>
  )
}
