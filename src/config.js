// Главные настройки сайта
export const GITHUB_USER = 'PainFrenemy2'
export const GITHUB_REPO = 'PainFrenemy2/null-code' // репозиторий, где каждая ветка — отдельный проект
export const VK_URL = 'https://vk.ru/null.code'
export const GITHUB_URL = `https://github.com/${GITHUB_REPO}`

// Ветки, которые НЕ являются проектами (сам сайт, служебные)
export const SKIP_BRANCHES = ['main', 'master', 'gh-pages']
export const SKIP_BRANCH_PREFIXES = ['claude/', 'dependabot/']

// Отдельные публичные репозитории, которые тоже показать во вкладке «Проекты»
// (формат 'owner/repo' или просто 'repo' — тогда берётся GITHUB_USER)
export const EXTRA_REPOS = ['monitoring-os']
