import { useRef } from 'react'
import { motion, useScroll, useTransform, useVelocity, useSpring } from 'framer-motion'

const words = ['OPEN SOURCE', 'TOOLS', 'MONITORING', 'MODS', 'BOTS', 'GAMEDEV', 'WEB', 'BACKEND', 'DESIGN', 'COMMUNITY']

function Row({ reverse, speed }) {
  const line = [...words, ...words]
  return (
    <div className="marquee__row">
      <motion.div
        className="marquee__track"
        animate={{ x: reverse ? ['-50%', '0%'] : ['0%', '-50%'] }}
        transition={{ duration: speed, repeat: Infinity, ease: 'linear' }}
      >
        {line.map((w, i) => (
          <span key={i} className={i % 2 ? 'outline' : ''}>
            {w} <em>/</em>
          </span>
        ))}
      </motion.div>
    </div>
  )
}

export default function Marquee() {
  const ref = useRef(null)
  const { scrollY } = useScroll()
  const vel = useSpring(useVelocity(scrollY), { damping: 50, stiffness: 300 })
  const skew = useTransform(vel, [-2000, 2000], [-8, 8])

  return (
    <motion.section className="marquee" ref={ref} style={{ skewY: skew }} aria-hidden>
      <Row speed={30} />
      <Row speed={36} reverse />
    </motion.section>
  )
}
