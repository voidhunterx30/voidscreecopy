import { motion } from "framer-motion"
import { Download } from "lucide-react"
import { Link } from "react-router-dom"
import { appRelease } from "../config/release"

function FinalCTA() {
  return (
    <section className="py-32 text-center max-w-3xl mx-auto px-6">
      <motion.div
        initial={{ opacity: 0, y: 24 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true }}
        transition={{ duration: 0.6 }}
        className="space-y-6"
      >
        <h2 className="text-4xl md:text-5xl font-bold">Ready to connect?</h2>

        <p className="text-text-secondary mt-4 text-lg max-w-xl mx-auto">
          Download voidscreecopy and start sharing your Android screen and camera today.
        </p>

        <div className="mt-8">
          <Link
            to="/download"
            className="inline-flex items-center gap-3 bg-blue-600 hover:bg-blue-500 text-white font-medium py-4 px-8 rounded-xl text-lg transition-colors"
          >
            <Download className="w-5 h-5" />
            Download for Windows
          </Link>
        </div>

        <p className="text-sm text-text-muted mt-6">
          v{appRelease.version} • {appRelease.platform} • {appRelease.installerSize}
        </p>
      </motion.div>
    </section>
  )
}

export default FinalCTA
