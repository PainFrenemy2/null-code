import { motion } from 'framer-motion'
import Logo from './Logo.jsx'
import { GITHUB_URL, VK_URL } from '../config.js'
import { goRoute, smoothTo } from '../router.js'

export default function Footer() {
  return (
    <footer className="footer">
      <div className="footer__row">
        <div className="footer__brand">
          <Logo size={48} />
          <div className="mono">
            null<span className="dim">/</span>code
            <div className="dim">community</div>
          </div>
        </div>
        <div className="footer__links mono">
          <a href="#/projects" onClick={goRoute('#/projects')}>Проекты</a>
          <a href={GITHUB_URL} target="_blank" rel="noreferrer">GitHub ↗</a>
          <a href={VK_URL} target="_blank" rel="noreferrer">VK ↗</a>
          <a href="#top" onClick={(e) => { e.preventDefault(); smoothTo(0) }}>Наверх ↑</a>
        </div>
      </div>
      <motion.div
        className="footer__giant"
        initial={{ y: '40%', opacity: 0 }}
        whileInView={{ y: 0, opacity: 1 }}
        viewport={{ once: true }}
        transition={{ duration: 1.2, ease: [0.22, 1, 0.36, 1] }}
      >
        NULL<span>/</span>CODE
      </motion.div>
      <div className="footer__bottom mono dim">
        <span>© {new Date().getFullYear()} null/code community</span>
        <span>from zero to code</span>
      </div>
    </footer>
  )
}
