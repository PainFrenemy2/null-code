import { useEffect, useState } from 'react'
import { AnimatePresence, motion } from 'framer-motion'
import Lenis from 'lenis'
import Preloader from './components/Preloader.jsx'
import Cursor from './components/Cursor.jsx'
import Nav from './components/Nav.jsx'
import Hero from './components/Hero.jsx'
import Marquee from './components/Marquee.jsx'
import Manifest from './components/Manifest.jsx'
import Services from './components/Services.jsx'
import ProjectsTeaser from './components/ProjectsTeaser.jsx'
import Process from './components/Process.jsx'
import Terminal from './components/Terminal.jsx'
import Stack from './components/Stack.jsx'
import Contact from './components/Contact.jsx'
import Footer from './components/Footer.jsx'
import Projects from './components/Projects.jsx'
import { smoothTo, useRoute } from './router.js'

const page = {
  initial: { opacity: 0, y: 30, filter: 'blur(8px)' },
  animate: { opacity: 1, y: 0, filter: 'blur(0px)', transition: { duration: 0.6, ease: [0.22, 1, 0.36, 1] } },
  exit: { opacity: 0, y: -20, filter: 'blur(8px)', transition: { duration: 0.3 } },
}

function Home({ ready }) {
  useEffect(() => {
    const id = window.__pendingSection
    if (!id) return
    window.__pendingSection = null
    setTimeout(() => smoothTo(id, true), 50)
  }, [])

  return (
    <>
      <Hero ready={ready} />
      <Marquee />
      <Manifest />
      <Services />
      <ProjectsTeaser />
      <Process />
      <Terminal />
      <Stack />
      <Contact />
    </>
  )
}

export default function App() {
  const route = useRoute()
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const lenis = new Lenis({ lerp: 0.1 })
    let id
    const raf = (t) => {
      lenis.raf(t)
      id = requestAnimationFrame(raf)
    }
    id = requestAnimationFrame(raf)
    window.__lenis = lenis
    return () => {
      cancelAnimationFrame(id)
      lenis.destroy()
    }
  }, [])

  useEffect(() => {
    document.body.style.overflow = loading ? 'hidden' : ''
  }, [loading])

  return (
    <>
      <AnimatePresence>{loading && <Preloader onDone={() => setLoading(false)} />}</AnimatePresence>
      <Cursor />
      <div className="noise" aria-hidden />
      <Nav route={route} />
      <AnimatePresence mode="wait" onExitComplete={() => smoothTo(0, true)}>
        <motion.main key={route} {...page}>
          {route === 'projects' ? <Projects ready={!loading} /> : <Home ready={!loading} />}
        </motion.main>
      </AnimatePresence>
      <Footer />
    </>
  )
}
