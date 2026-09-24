import { useMemo, useState } from 'react'
import { AnimatePresence, motion } from 'framer-motion'
import ProjectCard, { SkeletonCard, langColor } from './ProjectCard.jsx'
import Scramble from './Scramble.jsx'
import { useProjects } from '../github.js'
import { GITHUB_REPO, GITHUB_URL } from '../config.js'

const ease = [0.22, 1, 0.36, 1]

export default function Projects({ ready }) {
  const { status, projects, error, warning } = useProjects()
  const [q, setQ] = useState('')
  const [lang, setLang] = useState('all')
  const [onlyBuilds, setOnlyBuilds] = useState(false)

  const langs = useMemo(() => [...new Set(projects.map((p) => p.language).filter(Boolean))], [projects])
  const shown = projects.filter(
    (p) =>
      (lang === 'all' || p.language === lang) &&
      (!onlyBuilds || p.release?.assets.length) &&
      (!q || `${p.name} ${p.title || ''} ${p.description || ''} ${p.topics.join(' ')}`.toLowerCase().includes(q.toLowerCase())),
  )
  const builds = projects.filter((p) => p.release?.assets.length).length
  const downloads = projects.reduce((s, p) => s + (p.release?.assets.reduce((x, a) => x + a.downloads, 0) || 0), 0)

  return (
    <section className="projects section">
      <div className="projects__head">
        <div className="label mono">[ /projects ] всё открыто</div>
        <h1 className="projects__title">
          {['Бери', 'и', 'пользуйся'].map((w, i) => (
            <span className="letters__mask" key={w}>
              <motion.span
                className={i === 2 ? 'outline-text' : ''}
                initial={{ y: '110%' }}
                animate={ready ? { y: 0 } : {}}
                transition={{ duration: 0.9, delay: 0.1 + i * 0.08, ease }}
              >
                {w}
              </motion.span>
            </span>
          ))}
        </h1>
        <p className="projects__lead">
          Каждый проект сообщества null/code живёт в своей ветке на GitHub. Здесь можно сразу скачать готовую сборку под
          свою систему или исходники любой ветки.
        </p>

        <div className="projects__stats mono">
          <div>
            <b>{status === 'ok' ? projects.length : '—'}</b> проектов
          </div>
          <div>
            <b>{status === 'ok' ? builds : '—'}</b> со сборками
          </div>
          <div>
            <b>{status === 'ok' ? downloads : '—'}</b> скачиваний
          </div>
          <a href={GITHUB_URL} target="_blank" rel="noreferrer">
            github.com/{GITHUB_REPO} ↗
          </a>
        </div>
      </div>

      <div className="projects__controls">
        <label className="search mono">
          <span className="dim">$ grep</span>
          <input value={q} onChange={(e) => setQ(e.target.value)} placeholder="поиск по проектам..." />
        </label>
        <div className="projects__filters">
          <button className={`chip mono ${lang === 'all' ? 'is-on' : ''}`} onClick={() => setLang('all')}>
            все
          </button>
          {langs.map((l) => (
            <button key={l} className={`chip mono ${lang === l ? 'is-on' : ''}`} onClick={() => setLang(l)}>
              <i className="dot" style={{ background: langColor(l) }} />
              {l}
            </button>
          ))}
          <button className={`chip mono ${onlyBuilds ? 'is-on' : ''}`} onClick={() => setOnlyBuilds(!onlyBuilds)}>
            {onlyBuilds ? '✓' : '+'} только со сборками
          </button>
        </div>
      </div>

      {status === 'loading' && (
        <div className="pgrid">
          {[0, 1, 2, 3].map((i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      )}

      {status === 'error' && (
        <div className="pempty">
          <div className="mono dim">
            <Scramble
              text={
                error === 'rate-limit'
                  ? 'ERR 403: лимит запросов GitHub'
                  : error === 'private'
                    ? 'ERR 404: репозиторий закрыт'
                    : 'ERR: не удалось загрузить'
              }
            />
          </div>
          <p>
            {error === 'rate-limit'
              ? 'GitHub ограничивает число запросов с одного IP. Попробуйте через час или откройте проекты прямо на GitHub.'
              : error === 'private'
                ? 'Репозиторий с проектами пока приватный, поэтому список и загрузки недоступны. Скоро откроем.'
                : 'Проверьте подключение к интернету или откройте проекты прямо на GitHub.'}
          </p>
          <a className="btn" href={GITHUB_URL} target="_blank" rel="noreferrer">
            Открыть GitHub ↗
          </a>
        </div>
      )}

      {status === 'ok' && warning && (
        <div className="pwarn mono">
          <span>!</span>
          {warning === 'private'
            ? `Ветки ${GITHUB_REPO} сейчас скрыты: репозиторий приватный. Показаны только открытые проекты.`
            : warning === 'rate-limit'
              ? 'GitHub временно ограничил запросы, поэтому часть проектов не загрузилась. Обновите страницу позже.'
              : 'Часть проектов не загрузилась. Обновите страницу позже.'}
        </div>
      )}

      {status === 'ok' && (
        <motion.div className="pgrid" layout>
          <AnimatePresence mode="popLayout">
            {shown.map((p, i) => (
              <ProjectCard key={p.key} p={p} i={i} />
            ))}
          </AnimatePresence>
        </motion.div>
      )}

      {status === 'ok' && shown.length === 0 && (
        <div className="pempty">
          <div className="mono dim">// null — ничего не найдено</div>
          <p>Попробуйте другой запрос или сбросьте фильтры.</p>
        </div>
      )}
    </section>
  )
}
