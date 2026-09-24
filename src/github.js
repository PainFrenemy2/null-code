import { useEffect, useState } from 'react'
import { EXTRA_REPOS, GITHUB_REPO, GITHUB_USER, SKIP_BRANCHES, SKIP_BRANCH_PREFIXES } from './config.js'

const API = 'https://api.github.com'
const RAW = 'https://raw.githubusercontent.com'
const CACHE_KEY = `nc-projects-v2-${GITHUB_REPO}`
const CACHE_TTL = 10 * 60 * 1000

async function getJSON(url) {
  const res = await fetch(url, { headers: { Accept: 'application/vnd.github+json' } })
  if (res.status === 404) return null
  if (!res.ok) {
    const err = new Error(res.status === 403 || res.status === 429 ? 'rate-limit' : `http-${res.status}`)
    err.status = res.status
    throw err
  }
  return res.json()
}

function readCache() {
  try {
    const raw = sessionStorage.getItem(CACHE_KEY)
    if (!raw) return null
    const { at, data } = JSON.parse(raw)
    return Date.now() - at < CACHE_TTL ? data : null
  } catch {
    return null
  }
}

function writeCache(data) {
  try {
    sessionStorage.setItem(CACHE_KEY, JSON.stringify({ at: Date.now(), data }))
  } catch {
    /* приватный режим и т.п. — просто без кэша */
  }
}

const enc = (branch) => branch.split('/').map(encodeURIComponent).join('/')

const mapRelease = (release) =>
  release && {
    tag: release.tag_name,
    name: release.name,
    url: release.html_url,
    date: release.published_at,
    assets: (release.assets || []).map((a) => ({
      name: a.name,
      url: a.browser_download_url,
      size: a.size,
      downloads: a.download_count,
    })),
  }

// Первый абзац README ветки (заголовки, бейджи и картинки пропускаем)
async function readmeInfo(repo, branch) {
  try {
    const res = await fetch(`${RAW}/${repo}/${enc(branch)}/README.md`)
    if (!res.ok) return {}
    const text = await res.text()
    const title = text.match(/^#\s+(.+)$/m)?.[1]?.trim()
    const para = text
      .split(/\n\s*\n/)
      .map((b) => b.trim())
      .find((b) => b && !/^(#|!\[|\[!\[|<|```|---|\||>)/.test(b))
    const clean = para?.replace(/\[([^\]]+)\]\([^)]+\)/g, '$1').replace(/[*_`]/g, '').replace(/\s+/g, ' ')
    return { title, description: clean && clean.length > 220 ? clean.slice(0, 217) + '…' : clean }
  } catch {
    return {}
  }
}

// Релиз ветки (релизы приходят от новых к старым). Подходит, если:
// тег «ветка-v1.0» / «ветка/v1.0» / «ветка@1.0», релиз создан из ветки по имени
// или собран из одного из её последних коммитов (так делают CI-сборки с тегами вида v1.0.N)
function releaseFor(branch, releases, shas) {
  const b = branch.toLowerCase()
  return releases.find((r) => {
    if (r.draft) return false
    const tag = r.tag_name.toLowerCase()
    return (
      r.target_commitish === branch ||
      shas.has(r.target_commitish) ||
      [`${b}-`, `${b}/`, `${b}@`, `${b}_`].some((p) => tag.startsWith(p))
    )
  })
}

async function loadBranchProjects() {
  const branches = await getJSON(`${API}/repos/${GITHUB_REPO}/branches?per_page=100`)
  if (branches === null) {
    const err = new Error('private')
    throw err
  }
  const releases = (await getJSON(`${API}/repos/${GITHUB_REPO}/releases?per_page=100`)) || []
  const list = branches.filter(
    (b) => !SKIP_BRANCHES.includes(b.name) && !SKIP_BRANCH_PREFIXES.some((p) => b.name.startsWith(p)),
  )

  return Promise.all(
    list.map(async (b) => {
      const [info, commits] = await Promise.all([
        readmeInfo(GITHUB_REPO, b.name),
        getJSON(`${API}/repos/${GITHUB_REPO}/commits?sha=${encodeURIComponent(b.name)}&per_page=50`).catch(() => null),
      ])
      const commit = commits?.[0]
      const shas = new Set((commits || []).map((c) => c.sha).concat(b.commit.sha))
      return {
        kind: 'branch',
        key: `${GITHUB_REPO}#${b.name}`,
        name: b.name,
        title: info.title,
        full: GITHUB_REPO,
        description: info.description,
        url: `https://github.com/${GITHUB_REPO}/tree/${enc(b.name)}`,
        topics: [],
        pushed: commit?.commit?.committer?.date || null,
        lastCommit: commit?.commit?.message?.split('\n')[0],
        branch: b.name,
        zip: `https://github.com/${GITHUB_REPO}/archive/refs/heads/${enc(b.name)}.zip`,
        release: mapRelease(releaseFor(b.name, releases, shas)),
      }
    }),
  )
}

async function loadExtraRepo(name) {
  const full = name.includes('/') ? name : `${GITHUB_USER}/${name}`
  const r = await getJSON(`${API}/repos/${full}`)
  if (!r || r.private) return null
  const release = await getJSON(`${API}/repos/${full}/releases/latest`).catch(() => null)
  return {
    kind: 'repo',
    key: full,
    name: r.name,
    full,
    description: r.description,
    url: r.html_url,
    homepage: r.homepage,
    language: r.language,
    topics: r.topics || [],
    stars: r.stargazers_count,
    pushed: r.pushed_at,
    branch: r.default_branch,
    zip: `https://github.com/${full}/archive/refs/heads/${enc(r.default_branch)}.zip`,
    release: mapRelease(release),
  }
}

async function loadProjects() {
  const [branchRes, ...extra] = await Promise.allSettled([
    loadBranchProjects(),
    ...EXTRA_REPOS.map(loadExtraRepo),
  ])
  const projects = [
    ...(branchRes.status === 'fulfilled' ? branchRes.value : []),
    ...extra.filter((x) => x.status === 'fulfilled' && x.value).map((x) => x.value),
  ]
  // Совсем ничего не загрузилось — показываем причину
  if (!projects.length && branchRes.status === 'rejected') throw branchRes.reason
  const warning = branchRes.status === 'rejected' ? branchRes.reason.message : null

  // Сначала проекты с готовыми сборками, потом по свежести
  projects.sort(
    (a, b) => !!b.release?.assets.length - !!a.release?.assets.length || new Date(b.pushed || 0) - new Date(a.pushed || 0),
  )
  return { projects, warning }
}

export function useProjects() {
  const [state, setState] = useState(() => {
    const cached = readCache()
    return cached ? { status: 'ok', ...cached } : { status: 'loading', projects: [] }
  })

  useEffect(() => {
    if (state.status === 'ok') return
    let alive = true
    loadProjects()
      .then((data) => {
        if (!data.warning) writeCache(data)
        if (alive) setState({ status: 'ok', ...data })
      })
      .catch((e) => alive && setState({ status: 'error', error: e.message, projects: [] }))
    return () => {
      alive = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return state
}

export function detectOS() {
  const ua = navigator.userAgent.toLowerCase()
  if (/android/.test(ua)) return 'android'
  if (/iphone|ipad/.test(ua)) return 'ios'
  if (/win/.test(ua)) return 'windows'
  if (/mac/.test(ua)) return 'mac'
  if (/linux/.test(ua)) return 'linux'
  return null
}

export function assetOS(name) {
  const n = name.toLowerCase()
  if (/\.(exe|msi)$|win(dows)?[-_.]|[-_.]win|x86_64-pc-windows/.test(n)) return 'windows'
  if (/\.(dmg|pkg)$|mac|darwin|osx|apple/.test(n)) return 'mac'
  if (/\.apk$|android/.test(n)) return 'android'
  if (/\.(appimage|deb|rpm)$|linux/.test(n)) return 'linux'
  return null
}

export const OS_LABEL = { windows: 'Windows', mac: 'macOS', linux: 'Linux', android: 'Android', ios: 'iOS' }

export function formatSize(bytes) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 ** 2) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / 1024 ** 2).toFixed(1)} MB`
}

export function timeAgo(date) {
  if (!date) return 'недавно'
  const days = Math.floor((Date.now() - new Date(date)) / 864e5)
  if (days < 1) return 'сегодня'
  if (days < 2) return 'вчера'
  if (days < 30) return `${days} дн. назад`
  if (days < 365) return `${Math.floor(days / 30)} мес. назад`
  return `${Math.floor(days / 365)} г. назад`
}
