// Visual + audio celebration. Everything here is decoration: it respects prefers-reduced-motion,
// and sound only plays after the player switched it on (browsers block autoplay anyway).

// Emoji are gilded by CSS (.vsg-rain span), so every one of them falls in gold.
const EMOJI: Record<string, Array<string>> = {
    confetti: ["✨", "⭐", "🌟"],
    coins: ["💰", "🪙", "💎"],
    crown: ["👑", "💎", "✨"],
    eagle: ["🦅", "⭐", "✨"],
    fireworks: ["🎆", "✨", "🌟"],
    trophy: ["🏆", "🥇", "🏅"],
    legendary: ["👑", "🏆", "💰", "🦅", "💎", "🥇", "🏅", "🌟"],
}

export const reducedMotion = () => typeof window !== "undefined" && window.matchMedia?.("(prefers-reduced-motion: reduce)").matches

// A shower of gold leaf with a few gilded emoji in between.
export function celebrate(effect = "confetti") {
    const pool = EMOJI[effect] ?? EMOJI.confetti!
    const count = reducedMotion() ? 8 : effect === "legendary" ? 110 : 55
    const layer = document.createElement("div")
    layer.className = "vsg-rain"
    layer.setAttribute("aria-hidden", "true")
    for (let i = 0; i < count; i++) {
        const leaf = Math.random() < 0.7
        const el = document.createElement(leaf ? "i" : "span")
        if (leaf) {
            const size = 6 + Math.random() * 12
            el.style.width = `${size}px`
            el.style.height = `${size * (0.8 + Math.random() * 0.8)}px`
        } else {
            el.textContent = pool[Math.floor(Math.random() * pool.length)]!
            el.style.fontSize = `${1.3 + Math.random() * 1.6}rem`
        }
        el.style.left = `${Math.random() * 100}vw`
        el.style.animationDelay = `${Math.random() * 1.1}s`
        el.style.animationDuration = `${2.6 + Math.random() * 2.2}s`
        el.style.setProperty("--drift", `${(Math.random() - 0.5) * 30}vw`)
        el.style.setProperty("--spin", `${(Math.random() - 0.5) * 1080}deg`)
        el.style.setProperty("--flip", `${Math.round(Math.random() * 6) * 180}deg`)
        layer.appendChild(el)
    }
    document.body.appendChild(layer)
    if (effect === "legendary") document.body.classList.add("vsg-gold-flash")
    setTimeout(() => {
        layer.remove()
        document.body.classList.remove("vsg-gold-flash")
    }, 6000)
}

// ---- sound: synthesized with WebAudio, so there are no audio files to license ----
let ctx: AudioContext | undefined
function audio() {
    ctx ??= new AudioContext()
    if (ctx.state === "suspended") void ctx.resume()
    return ctx
}
function tone(freq: number, start: number, duration: number, type: OscillatorType = "square", gain = 0.08) {
    const a = audio()
    const osc = a.createOscillator()
    const g = a.createGain()
    osc.type = type
    osc.frequency.setValueAtTime(freq, a.currentTime + start)
    g.gain.setValueAtTime(0.0001, a.currentTime + start)
    g.gain.exponentialRampToValueAtTime(gain, a.currentTime + start + 0.02)
    g.gain.exponentialRampToValueAtTime(0.0001, a.currentTime + start + duration)
    osc.connect(g).connect(a.destination)
    osc.start(a.currentTime + start)
    osc.stop(a.currentTime + start + duration + 0.05)
    return osc
}

export const sounds = {
    fanfare() {
        // ta-ta-ta-TAAA
        ;[523, 523, 523, 659, 784, 1047].forEach((f, i) => tone(f, i < 3 ? i * 0.12 : 0.36 + (i - 3) * 0.16, i === 5 ? 0.7 : 0.14))
    },
    kaching() {
        tone(1568, 0, 0.12, "triangle", 0.12)
        tone(2093, 0.1, 0.45, "triangle", 0.12)
    },
    airhorn() {
        const osc = tone(466, 0, 0.9, "sawtooth", 0.06)
        const lfo = audio().createOscillator()
        const depth = audio().createGain()
        lfo.frequency.value = 9
        depth.gain.value = 12
        lfo.connect(depth).connect(osc.frequency)
        lfo.start()
        lfo.stop(audio().currentTime + 0.95)
    },
    boing() {
        const a = audio()
        const osc = tone(220, 0, 0.5, "sine", 0.12)
        osc.frequency.exponentialRampToValueAtTime(660, a.currentTime + 0.25)
    },
}

export function playFor(effect?: string) {
    switch (effect) {
        case "coins":
            return sounds.kaching()
        case "fireworks":
        case "legendary":
        case "trophy":
            return sounds.fanfare()
        case "crown":
            return sounds.airhorn()
        default:
            return Math.random() < 0.5 ? sounds.fanfare() : sounds.boing()
    }
}

export const pick = <T>(items: ReadonlyArray<T>): T => items[Math.floor(Math.random() * items.length)]!
