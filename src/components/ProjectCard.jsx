import { useMemo, useState } from 'react'
import { AnimatePresence, motion } from 'framer-motion'
import { OS_LABEL, assetOS, detectOS, formatSize, timeAgo } from '../github.js'

const LANG_COLORS = {
  JavaScript: '#f1e05a', TypeScript: '#3178c6', Python: '#3572A5', 'C++': '#f34b7d', C: '#555555', 'C#': '#178600',
  Go: '#00ADD8', Rust: '#dea584', Java: '#b07219', Kotlin: '#A97BFF', Lua: '#000080', PHP: '#4F5D95', Pawn: '#dbb284',
  HTML: '#e34c26', CSS: '#563d7c', Shell: '#89e051', Dart: '#00B4AB', Swift: '#F05138',
}

export const langColor = (l) => LANG_COLORS[l] || '#8a8a8a'

export default function ProjectCard({ p, i = 0 }) {
  const os = useMemo(detectOS, [])
  const [open, setOpen] = useState(false)
  const r = p.release
  const assets = r?.assets || []
  const best = assets.find((a) => assetOS(a.name) === os) || (assets.length === 1 ? assets[0] : null)
  const others = assets.filter((a) => a !== best)
  const downloads = assets.reduce((s, a) => s + a.downloads, 0)

  return (
    <motion.article
      className="pcard"
      layout
      initial={{ opacity: 0, y: 40 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.95 }}
      transition={{ duration: 0.6, delay: Math.min(i, 8) * 0.06, ease: [0.22, 1, 0.36, 1] }}
    >
      <div className="pcard__head">
        <div>
          <div className="pcard__meta mono">
            {p.language && (
              <span>
                <i className="dot" style={{ background: langColor(p.language) }} />
                {p.language}
              </span>
            )}
            {p.kind === 'branch' && <span>⎇ {p.branch}</span>}
            {p.stars !== undefined && <span>★ {p.stars}</span>}
            <span className="dim">обновлён {timeAgo(p.pushed)}</span>
          </div>
          <h3 className="pcard__name">{p.title || p.name}</h3>
        </div>
        {r ? (
          <span className="pcard__badge mono">{r.tag}</span>
        ) : (
          <span className="pcard__badge pcard__badge--dim mono">src</span>
        )}
      </div>

      <p className="pcard__desc">{p.description || 'Описание скоро появится.'}</p>

      {p.lastCommit && (
        <div className="pcard__commit mono">
          <span className="dim">последний коммит:</span> {p.lastCommit}
        </div>
      )}

      {p.topics.length > 0 && (
        <div className="pcard__topics mono">
          {p.topics.slice(0, 5).map((t) => (
            <span key={t}>#{t}</span>
          ))}
        </div>
      )}

      <div className="pcard__dl">
        {best && (
          <a className="btn pcard__main" href={best.url} data-hover>
            <span>
              ↓ Скачать{assetOS(best.name) ? ` для ${OS_LABEL[assetOS(best.name)]}` : ''}
              <small className="mono">
                {best.name} · {formatSize(best.size)}
              </small>
            </span>
          </a>
        )}

        {r && best && (
          <a className="pcard__src mono" href={p.zip}>
            ↓ исходники ветки {p.branch} (.zip)
          </a>
        )}

        {r && assets.length === 0 && (
          <a className="btn pcard__main" href={`https://github.com/${p.full}/archive/refs/tags/${r.tag}.zip`}>
            <span>
              ↓ Исходники {r.tag}
              <small className="mono">сборок в релизе нет · .zip</small>
            </span>
          </a>
        )}

        {!r && (
          <a className="btn btn--ghost pcard__main" href={p.zip}>
            <span>
              ↓ Исходный код
              <small className="mono">сборок пока нет · {p.branch.replace(/\//g, '-')}.zip</small>
            </span>
          </a>
        )}

        {others.length > 0 && (
          <>
            <button className="pcard__more mono" onClick={() => setOpen(!open)} data-hover>
              {best ? 'другие файлы' : 'выбрать файл'} ({others.length}) <span className={open ? 'rot' : ''}>▾</span>
            </button>
            <AnimatePresence initial={false}>
              {(open || !best) && (
                <motion.ul
                  className="pcard__assets mono"
                  initial={{ height: 0, opacity: 0 }}
                  animate={{ height: 'auto', opacity: 1 }}
                  exit={{ height: 0, opacity: 0 }}
                >
                  {others.map((a) => (
                    <li key={a.url}>
                      <a href={a.url}>
                        <span className="pcard__asset-name">{a.name}</span>
                        <span className="dim">
                          {assetOS(a.name) ? `${OS_LABEL[assetOS(a.name)]} · ` : ''}
                          {formatSize(a.size)}
                        </span>
                      </a>
                    </li>
                  ))}
                </motion.ul>
              )}
            </AnimatePresence>
          </>
        )}
      </div>

      <div className="pcard__foot mono">
        <span className="dim">
          {r ? `релиз ${timeAgo(r.date)}` : 'без релизов'}
          {downloads > 0 && ` · ${downloads} скачиваний`}
        </span>
        <span className="pcard__links">
          {p.homepage && (
            <a href={p.homepage} target="_blank" rel="noreferrer">
              сайт ↗
            </a>
          )}
          {r && (
            <a href={r.url} target="_blank" rel="noreferrer">
              релиз ↗
            </a>
          )}
          <a href={p.url} target="_blank" rel="noreferrer">
            GitHub ↗
          </a>
        </span>
      </div>
    </motion.article>
  )
}

export function SkeletonCard() {
  return (
    <div className="pcard pcard--skeleton" aria-hidden>
      <i style={{ width: '40%' }} />
      <i style={{ width: '70%', height: 28 }} />
      <i style={{ width: '90%' }} />
      <i style={{ width: '60%' }} />
      <i style={{ width: '100%', height: 56, marginTop: 'auto' }} />
    </div>
  )
}
