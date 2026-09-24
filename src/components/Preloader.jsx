import { useEffect, useState } from 'react'
import { motion } from 'framer-motion'
import Logo from './Logo.jsx'

export default function Preloader({ onDone }) {
  const [n, setN] = useState(0)

  useEffect(() => {
    let v = 0
    const id = setInterval(() => {
      v = Math.min(100, v + Math.ceil(Math.random() * 9))
      setN(v)
      if (v >= 100) {
        clearInterval(id)
        setTimeout(onDone, 450)
      }
    }, 55)
    return () => clearInterval(id)
  }, [onDone])

  return (
    <motion.div
      className="preloader"
      exit={{ clipPath: 'inset(0 0 100% 0)' }}
      transition={{ duration: 0.9, ease: [0.76, 0, 0.24, 1] }}
    >
      <Logo size={96} animate />
      <div className="preloader__row mono">
        <span>
          null<span className="dim">/</span>code
        </span>
        <span>{String(n).padStart(3, '0')}%</span>
      </div>
      <div className="preloader__bar">
        <motion.div className="preloader__fill" animate={{ scaleX: n / 100 }} />
      </div>
    </motion.div>
  )
}
