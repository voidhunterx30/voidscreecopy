import { motion } from "framer-motion";
import { Link } from "react-router-dom";
import { Download as DownloadIcon } from "lucide-react";
import { appRelease } from "../config/release";

const steps = [
  {
    label: "Step 1",
    title: "Download",
    content: `Click Download for Windows. The installer (${appRelease.installerSize}) will download directly.`,
  },
  {
    label: "Step 2",
    title: "Install",
    content: "Open voidscreecopy-installer.exe. Follow the installation wizard.",
  },
  {
    label: "Step 3",
    title: "Launch",
    content: "Open voidscreecopy from your Start Menu or desktop.",
  },
  {
    label: "Step 4",
    title: "Connect Android",
    content: "Connect your Android device via USB cable or Wi-Fi pairing.",
  },
  {
    label: "Step 5",
    title: "Start Sharing",
    content:
      "Choose Screen Mirroring, Camera Sharing, or TV Casting from the app.",
  },
  {
    label: "Step 6",
    title: "OBS Setup",
    content:
      "In OBS, add a Window Capture source and select the voidscreecopy window.",
  },
];

const container = {
  hidden: {},
  show: {
    transition: {
      staggerChildren: 0.1,
    },
  },
};

const item = {
  hidden: { opacity: 0, y: 16 },
  show: { opacity: 1, y: 0, transition: { duration: 0.4 } },
};

export default function GettingStarted() {
  return (
    <section id="getting-started" className="py-32 bg-bg-elevated">
      <div className="max-w-4xl mx-auto px-6">
        <h2 className="text-3xl md:text-4xl font-bold mb-4">
          Getting Started
        </h2>
        <p className="text-text-secondary mb-16">
          Everything you need to go from download to first mirror.
        </p>

        <motion.div
          className="space-y-4"
          variants={container}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true, margin: "-100px" }}
        >
          {steps.map((step) => (
            <motion.div
              key={step.label}
              variants={item}
              className="bg-bg-card border border-border rounded-xl p-6"
            >
              <span className="text-accent font-semibold text-sm">
                {step.label}
              </span>
              <h3 className="text-lg font-semibold mt-2">{step.title}</h3>
              <p className="text-text-secondary mt-2">{step.content}</p>
            </motion.div>
          ))}
        </motion.div>

        <motion.div
          className="mt-12 text-center"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          transition={{ delay: 0.6 }}
        >
          <Link
            to="/download"
            className="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-3 rounded-lg transition-colors"
          >
            <DownloadIcon className="w-4 h-4" />
            Download for Windows
          </Link>
        </motion.div>
      </div>
    </section>
  );
}
