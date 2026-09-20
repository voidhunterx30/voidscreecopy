import { motion } from "framer-motion";
import { Download, ArrowRight, Monitor, Camera, Wifi } from "lucide-react";
import { Link } from "react-router-dom";
import { appRelease } from "../config/release";
import { useNavigateToSection } from "../lib/navigation";

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0 },
};

const stagger = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.1 } },
};

export default function Hero() {
  const navigateToSection = useNavigateToSection();

  return (
    <section className="relative min-h-screen flex items-center justify-center overflow-hidden bg-bg">
      {/* Grid pattern background */}
      <div
        className="pointer-events-none absolute inset-0"
        style={{
          backgroundImage:
            "linear-gradient(rgba(255,255,255,0.03) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.03) 1px, transparent 1px)",
          backgroundSize: "64px 64px",
        }}
      />

      {/* Radial glow */}
      <div
        className="pointer-events-none absolute top-1/2 left-1/2 -z-10 h-[600px] w-[600px] -translate-x-1/2 -translate-y-1/2 rounded-full opacity-20 blur-3xl"
        style={{
          background:
            "radial-gradient(circle, var(--color-accent) 0%, transparent 70%)",
        }}
      />

      <motion.div
        variants={stagger}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true, amount: 0.2 }}
        className="relative z-10 mx-auto flex max-w-5xl flex-col items-center px-6 py-24 text-center"
      >
        {/* Eyebrow */}
        <motion.p
          variants={fadeUp}
          className="mb-6 font-mono text-xs uppercase tracking-[0.25em] text-accent"
        >
          WINDOWS SOFTWARE • v{appRelease.version}
        </motion.p>

        {/* Headline */}
        <motion.h1
          variants={fadeUp}
          className="mb-6 text-5xl font-extrabold leading-[0.95] text-text-primary md:text-7xl lg:text-8xl"
          style={{ textWrap: "balance" }}
        >
          Your Android.
          <br />
          Your Screen.
          <br />
          Your Camera.
        </motion.h1>

        {/* Supporting text */}
        <motion.p
          variants={fadeUp}
          className="mb-10 max-w-xl text-lg text-text-secondary"
        >
          Low-latency Android screen, camera, and TV sharing for Windows and
          OBS.
        </motion.p>

        {/* CTAs */}
        <motion.div
          variants={fadeUp}
          className="mb-4 flex flex-col items-center gap-4 sm:flex-row"
        >
          <Link
            to="/download"
            className="inline-flex items-center gap-2 rounded-lg bg-accent px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-accent/20 transition-colors hover:bg-accent-hover"
          >
            <Download className="h-4 w-4" />
            Download for Windows
          </Link>
          <button
            onClick={() => navigateToSection("how-it-works")}
            className="inline-flex items-center gap-2 rounded-lg border border-border px-6 py-3 text-sm font-semibold text-text-secondary transition-colors hover:border-border-strong hover:text-text-primary cursor-pointer bg-transparent"
          >
            See how it works
            <ArrowRight className="h-4 w-4" />
          </button>
        </motion.div>

        {/* Metadata */}
        <motion.p
          variants={fadeUp}
          className="mb-20 font-mono text-sm text-text-muted"
        >
          {appRelease.platform} • {appRelease.architecture} •{" "}
          {appRelease.installerSize}
        </motion.p>

        {/* Product visualization */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5, duration: 0.6 }}
          viewport={{ once: true }}
          className="flex w-full max-w-3xl flex-col items-center gap-6 md:flex-row md:items-stretch md:justify-center md:gap-0"
        >
          {/* Android box */}
          <div className="flex flex-1 flex-col items-center justify-center gap-3 rounded-xl border border-border bg-bg-card p-6 md:rounded-r-none">
            <Monitor className="h-10 w-10 text-text-secondary" />
            <span className="text-sm font-semibold text-text-primary">
              Android
            </span>
            <span className="text-xs text-text-muted">Device</span>
          </div>

          {/* Connection line left */}
          <div className="hidden items-center md:flex">
            <motion.div
              initial={{ width: 0 }}
              whileInView={{ width: 48 }}
              transition={{ delay: 0.8, duration: 0.5 }}
              viewport={{ once: true }}
              className="h-px bg-accent"
            />
          </div>

          {/* Center box */}
          <div className="relative flex flex-1 flex-col items-center justify-center gap-3 rounded-xl border-2 border-accent bg-accent-subtle p-6">
            <Wifi className="h-10 w-10 text-accent" />
            <span className="text-sm font-bold text-text-primary">
              voidscreecopy
            </span>
            <span className="text-xs text-accent">Bridge</span>
          </div>

          {/* Connection line right */}
          <div className="hidden items-center md:flex">
            <motion.div
              initial={{ width: 0 }}
              whileInView={{ width: 48 }}
              transition={{ delay: 1.0, duration: 0.5 }}
              viewport={{ once: true }}
              className="h-px bg-accent"
            />
          </div>

          {/* OBS box */}
          <div className="flex flex-1 flex-col items-center justify-center gap-3 rounded-xl border border-border bg-bg-card p-6 md:rounded-l-none">
            <Camera className="h-10 w-10 text-text-secondary" />
            <span className="text-sm font-semibold text-text-primary">
              OBS
            </span>
            <span className="text-xs text-text-muted">Windows</span>
          </div>
        </motion.div>

        {/* Connection lines on mobile (stacked vertically) */}
        <div className="flex flex-col items-center gap-0 md:hidden">
          <motion.div
            initial={{ height: 0 }}
            whileInView={{ height: 32 }}
            transition={{ delay: 0.8, duration: 0.4 }}
            viewport={{ once: true }}
            className="w-px bg-accent"
          />
          <motion.div
            initial={{ height: 0 }}
            whileInView={{ height: 32 }}
            transition={{ delay: 1.0, duration: 0.4 }}
            viewport={{ once: true }}
            className="w-px bg-accent"
          />
        </div>
      </motion.div>
    </section>
  );
}
