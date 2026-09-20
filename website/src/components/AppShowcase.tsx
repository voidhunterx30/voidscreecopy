import { motion, useScroll, useTransform } from "framer-motion"
import { useRef } from "react"

function AppShowcase() {
  const containerRef = useRef<HTMLDivElement>(null)
  const { scrollYProgress } = useScroll({
    target: containerRef,
    offset: ["start end", "end start"]
  })

  const y = useTransform(scrollYProgress, [0, 1], [0, -40])

  return (
    <section className="py-32 overflow-hidden max-w-7xl mx-auto px-6" ref={containerRef}>
      <motion.h2
        initial={{ opacity: 0, y: 20 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true }}
        transition={{ duration: 0.6 }}
        className="text-4xl md:text-5xl font-bold text-center mb-16"
      >
        See voidscreecopy in action
      </motion.h2>

      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        whileInView={{ opacity: 1, scale: 1 }}
        viewport={{ once: true }}
        transition={{ duration: 0.7, ease: "easeOut" }}
        style={{
          perspective: "1200px",
          perspectiveOrigin: "center"
        }}
        className="w-full max-w-5xl mx-auto"
      >
        <motion.div style={{ y }} className="w-full">
          {/* App Window */}
          <div
            className="rounded-xl overflow-hidden border border-white/10 shadow-2xl"
            style={{ background: "#131416" }}
          >
            {/* Title Bar */}
            <div className="flex items-center gap-2 px-4 py-3 border-b border-white/5" style={{ background: "#1a1c1e" }}>
              <div className="w-3 h-3 rounded-full bg-red-500" />
              <div className="w-3 h-3 rounded-full bg-yellow-500" />
              <div className="w-3 h-3 rounded-full bg-green-500" />
              <span className="ml-3 text-xs text-text-muted font-medium tracking-wide">
                voidscreecopy
              </span>
            </div>

            {/* App Content */}
            <div className="flex flex-col lg:flex-row min-h-[420px]">
              {/* Left Sidebar */}
              <div className="w-full lg:w-56 border-b lg:border-b-0 lg:border-r border-white/5 p-4 flex lg:flex-col gap-1">
                {[
                  { icon: "◻", label: "Screen" },
                  { icon: "◎", label: "Camera" },
                  { icon: "⊞", label: "TV" },
                  { icon: "⚙", label: "Settings" },
                  { icon: "⚡", label: "Admin" }
                ].map((item) => (
                  <div
                    key={item.label}
                    className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-text-secondary hover:bg-white/5 transition-colors"
                  >
                    <span className="text-text-muted w-4 text-center text-xs">{item.icon}</span>
                    <span>{item.label}</span>
                  </div>
                ))}
              </div>

              {/* Main Content */}
              <div className="flex-1 p-6 space-y-6">
                <h3 className="text-lg font-semibold text-white">Screen Mirroring</h3>

                {/* Device Setup Card */}
                <div className="rounded-lg border border-white/10 p-5 space-y-4" style={{ background: "#1a1c1e" }}>
                  <p className="text-sm font-medium text-white">Device Setup</p>
                  <div className="space-y-3">
                    <div className="rounded-md border border-white/10 px-4 py-2.5 text-sm text-text-muted flex items-center justify-between" style={{ background: "#131416" }}>
                      <span>Select device...</span>
                      <span className="text-xs">▾</span>
                    </div>
                    <div className="rounded-md border border-white/10 px-4 py-2.5 text-sm text-text-muted flex items-center justify-between" style={{ background: "#131416" }}>
                      <span>Select OBS window...</span>
                      <span className="text-xs">▾</span>
                    </div>
                  </div>
                </div>

                {/* Connected Devices */}
                <div>
                  <p className="text-sm font-medium text-white mb-3">Connected Devices</p>
                  <div className="rounded-lg border border-dashed border-white/10 p-6 text-center">
                    <p className="text-sm text-text-muted">No devices connected</p>
                  </div>
                </div>

                {/* Start Button */}
                <button
                  className="w-full py-3 px-6 rounded-lg font-medium text-white text-sm transition-opacity"
                  style={{ background: "#2563eb" }}
                >
                  Start OBS Share
                </button>
              </div>

              {/* Right Panel */}
              <div className="w-full lg:w-64 border-t lg:border-t-0 lg:border-l border-white/5 p-4 space-y-6">
                <div>
                  <p className="text-xs font-medium text-text-muted uppercase tracking-wider mb-3">
                    OBS Window Capture
                  </p>
                  <div className="space-y-2">
                    <div className="flex justify-between text-xs">
                      <span className="text-text-muted">Status</span>
                      <span className="text-green-400">Ready</span>
                    </div>
                    <div className="flex justify-between text-xs">
                      <span className="text-text-muted">Resolution</span>
                      <span className="text-text-secondary">1920×1080</span>
                    </div>
                    <div className="flex justify-between text-xs">
                      <span className="text-text-muted">FPS</span>
                      <span className="text-text-secondary">60</span>
                    </div>
                  </div>
                </div>

                <div>
                  <p className="text-xs font-medium text-text-muted uppercase tracking-wider mb-3">
                    Activity Log
                  </p>
                  <div
                    className="rounded-md p-3 space-y-1.5 text-xs border border-white/5"
                    style={{
                      background: "#0e0f11",
                      fontFamily: "Consolas, 'Courier New', monospace"
                    }}
                  >
                    <p className="text-text-muted">[14:32:01] App started</p>
                    <p className="text-text-muted">[14:32:01] ADB initialized</p>
                    <p className="text-text-muted">[14:32:02] Scanning devices...</p>
                    <p className="text-green-400/70">[14:32:03] Ready</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </motion.div>
      </motion.div>
    </section>
  )
}

export default AppShowcase
