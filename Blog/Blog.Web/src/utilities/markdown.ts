// A deliberately small, safe Markdown subset for article bodies:
//   ## / ### headings, paragraphs, "- " / "* " lists, "1. " lists, "> " quotes, **bold**, *italic*, `code`, [text](https://...)
// Everything is HTML-escaped first, so the output is safe to bind with v-html.

const escapeHtml = (s: string) => s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#39;")

function inline(text: string): string {
    let s = escapeHtml(text)
    s = s.replace(/`([^`]+)`/g, "<code>$1</code>")
    s = s.replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>")
    s = s.replace(/(^|[^*])\*([^*\s][^*]*)\*/g, "$1<em>$2</em>")
    s = s.replace(/\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/g, '<a href="$2" target="_blank" rel="noopener noreferrer">$1</a>')
    return s
}

export function renderMarkdown(source?: string): string {
    if (!source) return ""
    const lines = source.replace(/\r\n?/g, "\n").split("\n")
    const out: Array<string> = []
    let paragraph: Array<string> = []
    let list: { tag: "ul" | "ol"; items: Array<string> } | undefined
    let quote: Array<string> = []

    const flushParagraph = () => {
        if (paragraph.length) out.push(`<p>${inline(paragraph.join(" "))}</p>`)
        paragraph = []
    }
    const flushList = () => {
        if (list) out.push(`<${list.tag}>${list.items.map((i) => `<li>${inline(i)}</li>`).join("")}</${list.tag}>`)
        list = undefined
    }
    const flushQuote = () => {
        if (quote.length) out.push(`<blockquote><p>${inline(quote.join(" "))}</p></blockquote>`)
        quote = []
    }
    const flushAll = () => {
        flushParagraph()
        flushList()
        flushQuote()
    }

    for (const raw of lines) {
        const line = raw.trimEnd()
        let m: RegExpMatchArray | null
        if (!line.trim()) {
            flushAll()
        } else if ((m = line.match(/^(#{2,4})\s+(.*)$/))) {
            flushAll()
            const level = m[1]!.length
            out.push(`<h${level}>${inline(m[2]!)}</h${level}>`)
        } else if ((m = line.match(/^\s*[-*]\s+(.*)$/))) {
            flushParagraph()
            flushQuote()
            if (list?.tag !== "ul") {
                flushList()
                list = { tag: "ul", items: [] }
            }
            list.items.push(m[1]!)
        } else if ((m = line.match(/^\s*\d+\.\s+(.*)$/))) {
            flushParagraph()
            flushQuote()
            if (list?.tag !== "ol") {
                flushList()
                list = { tag: "ol", items: [] }
            }
            list.items.push(m[1]!)
        } else if ((m = line.match(/^>\s?(.*)$/))) {
            flushParagraph()
            flushList()
            quote.push(m[1]!)
        } else {
            flushList()
            flushQuote()
            paragraph.push(line.trim())
        }
    }
    flushAll()
    return out.join("\n")
}

/** Rough reading time, mirroring the API's 220 words-per-minute estimate. */
export function estimateReadingTime(source?: string): number {
    const words = source?.match(/\b\w+\b/g)?.length ?? 0
    return words === 0 ? 0 : Math.max(1, Math.round(words / 220))
}
