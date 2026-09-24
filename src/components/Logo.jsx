import { motion } from 'framer-motion'

// Фирменная «N» с косым срезом
export default function Logo({ size = 40, animate = false, className = '' }) {
  const draw = (delay) =>
    animate
      ? {
          initial: { pathLength: 0, fillOpacity: 0 },
          animate: { pathLength: 1, fillOpacity: 1 },
          transition: {
            pathLength: { duration: 1, delay, ease: [0.65, 0, 0.35, 1] },
            fillOpacity: { duration: 0.4, delay: delay + 0.8 },
          },
        }
      : {}

  return (
    <svg className={className} width={size} height={size} viewBox="0 0 100 100" aria-label="null/code">
      <defs>
        <mask id="nc-cut">
          <rect width="100" height="100" fill="#fff" />
          <path d="M28 90 74 10" stroke="#000" strokeWidth="5" />
        </mask>
      </defs>
      <g mask="url(#nc-cut)" fill="currentColor" stroke="currentColor" strokeWidth="1.5">
        <motion.path d="M20 12h16v76H20z" {...draw(0)} />
        <motion.path d="M64 12h16v76H64z" {...draw(0.15)} />
        <motion.path d="M20 12h18l42 76H62z" {...draw(0.3)} />
      </g>
    </svg>
  )
}
