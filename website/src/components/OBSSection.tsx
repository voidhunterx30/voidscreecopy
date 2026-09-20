import { motion } from "framer-motion";
import { ArrowRight } from "lucide-react";

const pipeline = [
  { label: "Android", icon: "Monitor" },
  { label: "voidscreecopy", accent: true },
  { label: "OBS", icon: "Radio" },
  { label: "Stream / Record" },
];

const colVariants = {
  hidden: { opacity: 0, y: 24 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5 } },
};

export default function OBSSection() {
  return (
    <section id="obs" className="py-32">
      <div className="max-w-7xl mx-auto px-6 lg:grid lg:grid-cols-2 lg:gap-16 items-center">
        <motion.div
          variants={colVariants}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true, margin: "-100px" }}
        >
          <h2 className="text-4xl font-bold">
            Built for OBS workflows.
          </h2>
          <p className="text-text-secondary mt-4 text-lg">
            Bring supported Android screen and camera workflows into your
            Windows production setup with voidscreecopy.
          </p>
          <a
            href="#getting-started"
            className="inline-flex items-center gap-2 mt-8 border border-border rounded-lg px-5 py-2.5 font-semibold hover:bg-bg-elevated transition-colors"
          >
            Learn the OBS setup
            <ArrowRight className="w-4 h-4" />
          </a>
        </motion.div>

        <motion.div
          className="mt-12 lg:mt-0"
          variants={colVariants}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true, margin: "-100px" }}
          transition={{ delay: 0.15 }}
        >
          <div className="flex flex-col items-center gap-0">
            {pipeline.map((node, i) => (
              <div key={node.label} className="flex flex-col items-center">
                <motion.div
                  className={`w-full max-w-[220px] rounded-xl border p-4 text-center font-semibold ${
                    node.accent
                      ? "border-accent bg-accent/10 text-accent"
                      : "border-border bg-bg-card"
                  }`}
                  initial={{ opacity: 0, scale: 0.9 }}
                  whileInView={{ opacity: 1, scale: 1 }}
                  viewport={{ once: true }}
                  transition={{ delay: i * 0.12, duration: 0.4 }}
                >
                  {node.label}
                </motion.div>

                {i < pipeline.length - 1 && (
                  <motion.div
                    className="w-px h-8 bg-border"
                    initial={{ scaleY: 0 }}
                    whileInView={{ scaleY: 1 }}
                    viewport={{ once: true }}
                    transition={{ delay: i * 0.12 + 0.08, duration: 0.3 }}
                  />
                )}
              </div>
            ))}
          </div>
        </motion.div>
      </div>
    </section>
  );
}
