import { motion } from 'framer-motion'
import Logo from './Logo.jsx'
import { VK_URL } from './Contact.jsx'
import { scrollTo } from './Nav.jsx'

export default function Footer() {
  return (
    <footer className="footer">
      <div className="footer__row">
        <div className="footer__brand">
          <Logo size={48} />
          <div className="mono">
            null<span className="dim">/</span>code
            <div className="dim">official</div>
          </div>
        </div>
        <div className="footer__links mono">
          <a href={VK_URL} target="_blank" rel="noreferrer">VK ↗</a>
          <a href="#top" onClick={scrollTo('#top')}>Наверх ↑</a>
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
        <span>© {new Date().getFullYear()} null/code</span>
        <span>from zero to code</span>
      </div>
    </footer>
  )
}
