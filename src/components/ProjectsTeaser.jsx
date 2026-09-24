import { motion } from 'framer-motion'
import Reveal from './Reveal.jsx'
import Magnetic from './Magnetic.jsx'
import { langColor } from './ProjectCard.jsx'
import { useProjects, timeAgo } from '../github.js'
import { goRoute } from '../router.js'

export default function ProjectsTeaser() {
  const { status, projects } = useProjects()
  const top = projects.slice(0, 3)

  return (
    <section className="section teaser" id="projects">
      <div className="section__head">
        <div className="label mono">[ 02 ] проекты</div>
        <Reveal as="h2" className="h2">
          Сделали — <span className="outline-text">выложили</span>,<br />
          а ты скачал
        </Reveal>
      </div>

      <div className="teaser__list">
        {status === 'loading' &&
          [0, 1, 2].map((i) => <div key={i} className="trow trow--skeleton" />)}
        {top.map((p, i) => (
          <motion.a
            key={p.key}
            href="#/projects"
            onClick={goRoute('#/projects')}
            className="trow"
            data-hover
            initial={{ opacity: 0, x: -40 }}
            whileInView={{ opacity: 1, x: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.7, delay: i * 0.1, ease: [0.22, 1, 0.36, 1] }}
          >
            <span className="trow__n mono dim">0{i + 1}</span>
            <span className="trow__name">{p.title || p.name}</span>
            <span className="trow__desc">{p.description || '—'}</span>
            <span className="trow__meta mono">
              {p.language && <i className="dot" style={{ background: langColor(p.language) }} />}
              {p.release ? p.release.tag : 'src'} · {timeAgo(p.pushed)}
            </span>
            <span className="trow__arrow">↓</span>
          </motion.a>
        ))}
      </div>

      <Reveal className="teaser__cta">
        <Magnetic>
          <a href="#/projects" className="btn btn--big" onClick={goRoute('#/projects')}>
            Все проекты и загрузки <span className="arrow">→</span>
          </a>
        </Magnetic>
      </Reveal>
    </section>
  )
}
