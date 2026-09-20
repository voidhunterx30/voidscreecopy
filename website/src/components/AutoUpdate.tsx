import { motion } from "framer-motion"
import { RefreshCw, ArrowDown } from "lucide-react"
import { appRelease } from "../config/release"

const steps = [
  {
    label: `v${appRelease.version}`,
    sublabel: "Current version",
    style: "border border-white/10",
    bg: "#1a1c1e"
  },
  {
    label: "Update available",
    sublabel: "New version detected",
    style: "border border-accent/50",
    bg: "#1a1c1e"
  },
  {
    label: "Download & install",
    sublabel: "One click",
    style: "border-0",
    bg: "#2563eb"
  },
  {
    label: "Updated",
    sublabel: "✓ Restart & enjoy",
    style: "border border-green-500/30",
    bg: "#1a1c1e",
    accent: true
  }
]

function AutoUpdate() {
  return (
    <section className="py-32 max-w-5xl mx-auto px-6">
      <div className="grid lg:grid-cols-2 gap-16 items-center">
        {/* Left */}
        <div>
          <motion.h2
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
            className="text-3xl md:text-4xl font-bold"
          >
            Always ready for the next version.
          </motion.h2>
          <motion.p
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6, delay: 0.1 }}
            className="text-text-secondary mt-4 text-lg"
          >
            voidscreecopy includes an automatic update system. When a new version is
            available, the app notifies you — one click to update.
          </motion.p>
          <motion.div
            initial={{ opacity: 0 }}
            whileInView={{ opacity: 1 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5, delay: 0.3 }}
            className="mt-8 flex items-center gap-2 text-text-muted text-sm"
          >
            <RefreshCw className="w-4 h-4" />
            <span>Auto-update built in — no manual downloads</span>
          </motion.div>
        </div>

        {/* Right — flow visualization */}
        <div className="flex flex-col items-center gap-2">
          {steps.map((step, i) => (
            <motion.div
              key={step.label}
              initial={{ opacity: 0, y: 16 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: 0.15 * i }}
              className="flex flex-col items-center"
            >
              <div
                className={`w-full max-w-[240px] rounded-xl px-5 py-3.5 text-center ${step.style}`}
                style={{ background: step.bg }}
              >
                <p className={`text-sm font-semibold ${step.accent ? "text-green-400" : "text-white"}`}>
                  {step.label}
                </p>
                <p className="text-xs text-text-muted mt-0.5">{step.sublabel}</p>
              </div>

              {i < steps.length - 1 && (
                <motion.div
                  initial={{ opacity: 0 }}
                  whileInView={{ opacity: 1 }}
                  viewport={{ once: true }}
                  transition={{ duration: 0.3, delay: 0.15 * i + 0.1 }}
                  className="my-1"
                >
                  <ArrowDown className="w-4 h-4 text-text-muted" />
                </motion.div>
              )}
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  )
}

export default AutoUpdate
