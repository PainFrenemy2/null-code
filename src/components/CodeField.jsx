import { useEffect, useRef } from 'react'

// Сетка символов, которая «просыпается» вокруг курсора
export default function CodeField() {
  const ref = useRef(null)

  useEffect(() => {
    const canvas = ref.current
    const ctx = canvas.getContext('2d')
    const glyphs = '01/\\{}<>;=_NULL'
    const step = 22
    let w, h, cols, rows, cells, raf
    const mouse = { x: -9999, y: -9999, tx: -9999, ty: -9999 }
    const dpr = Math.min(window.devicePixelRatio || 1, 2)
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches

    const resize = () => {
      w = canvas.clientWidth
      h = canvas.clientHeight
      canvas.width = w * dpr
      canvas.height = h * dpr
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0)
      cols = Math.ceil(w / step)
      rows = Math.ceil(h / step)
      cells = Array.from({ length: cols * rows }, () => ({
        ch: glyphs[(Math.random() * glyphs.length) | 0],
        glow: 0,
        base: Math.random() * 0.08,
      }))
    }

    const move = (e) => {
      const r = canvas.getBoundingClientRect()
      mouse.tx = e.clientX - r.left
      mouse.ty = e.clientY - r.top
    }

    let t = 0
    const tick = () => {
      t += 1
      mouse.x += (mouse.tx - mouse.x) * 0.15
      mouse.y += (mouse.ty - mouse.y) * 0.15
      ctx.clearRect(0, 0, w, h)
      ctx.font = '12px "JetBrains Mono", monospace'
      ctx.textAlign = 'center'
      ctx.textBaseline = 'middle'
      const wave = (t * 3) % (h + 400) - 200
      for (let j = 0; j < rows; j++) {
        for (let i = 0; i < cols; i++) {
          const c = cells[j * cols + i]
          const x = i * step + step / 2
          const y = j * step + step / 2
          const d = Math.hypot(x - mouse.x, y - mouse.y)
          const near = Math.max(0, 1 - d / 180)
          const scan = Math.max(0, 1 - Math.abs(y - wave) / 60) * 0.25
          c.glow = Math.max(near, c.glow * 0.94)
          if (Math.random() < 0.002 + near * 0.08) c.ch = glyphs[(Math.random() * glyphs.length) | 0]
          const a = c.base + c.glow * 0.85 + scan
          if (a < 0.02) continue
          ctx.fillStyle = `rgba(232,232,232,${a})`
          ctx.fillText(c.ch, x, y)
        }
      }
      raf = requestAnimationFrame(tick)
    }

    resize()
    window.addEventListener('resize', resize)
    window.addEventListener('pointermove', move)
    if (reduce) {
      tick()
      cancelAnimationFrame(raf)
    } else tick()

    return () => {
      cancelAnimationFrame(raf)
      window.removeEventListener('resize', resize)
      window.removeEventListener('pointermove', move)
    }
  }, [])

  return <canvas ref={ref} className="codefield" aria-hidden />
}
