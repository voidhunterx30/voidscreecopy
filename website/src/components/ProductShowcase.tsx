import { motion } from "framer-motion";
import { Monitor, Camera, Tv, Radio } from "lucide-react";

const cards = [
  {
    icon: Monitor,
    title: "Screen Mirroring",
    description:
      "Mirror your Android device screen to Windows. Share your phone or tablet display directly.",
  },
  {
    icon: Camera,
    title: "Camera Sharing",
    description:
      "Use your Android camera as part of your Windows workflow. Share camera feeds.",
  },
  {
    icon: Tv,
    title: "TV Casting",
    description:
      "Cast your Android content to supported TVs over Wi-Fi.",
  },
  {
    icon: Radio,
    title: "OBS Integration",
    description:
      "Bring Android screen and camera feeds into OBS for streaming and recording.",
  },
];

const container = {
  hidden: {},
  show: { transition: { staggerChildren: 0.12 } },
};

const fadeUp = {
  hidden: { opacity: 0, y: 32 },
  show: { opacity: 1, y: 0, transition: { duration: 0.6, ease: "easeOut" as const } },
};

const lineGrow = {
  hidden: { scaleX: 0 },
  show: { scaleX: 1, transition: { duration: 0.8, ease: "easeOut" as const } },
};

const steps = ["Android", "voidscreecopy", "Screen / Camera / TV / OBS"];

export default function ProductShowcase() {
  return (
    <section id="product" className="py-32">
      <div className="max-w-7xl mx-auto px-6">
        <motion.h2
          initial={{ opacity: 0, y: 24 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, amount: 0.4 }}
          transition={{ duration: 0.5 }}
          className="text-4xl md:text-5xl font-bold text-center mb-4"
        >
          One app. Multiple ways to connect.
        </motion.h2>

        <motion.p
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true, amount: 0.4 }}
          transition={{ duration: 0.5, delay: 0.15 }}
          className="text-text-secondary text-center mb-20"
        >
          Mirror your screen. Share your camera. Cast to TV.
        </motion.p>

        <motion.div
          variants={container}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true, amount: 0.2 }}
          className="grid md:grid-cols-2 gap-6"
        >
          {cards.map((card) => (
            <motion.div
              key={card.title}
              variants={fadeUp}
              className="bg-bg-card border border-border rounded-2xl p-8"
            >
              <div className="w-12 h-12 rounded-xl bg-accent-subtle flex items-center justify-center">
                <card.icon className="w-6 h-6 text-accent" />
              </div>
              <h3 className="text-xl font-semibold mt-6">{card.title}</h3>
              <p className="text-text-secondary mt-3">{card.description}</p>
            </motion.div>
          ))}
        </motion.div>

        <div className="mt-24">
          <div className="flex flex-col md:flex-row items-center justify-center gap-4 md:gap-0">
            {steps.map((step, i) => (
              <div key={step} className="flex items-center">
                <span className="bg-bg-card border border-border rounded-full px-5 py-2 text-sm font-medium whitespace-nowrap">
                  {step}
                </span>
                {i < steps.length - 1 && (
                  <motion.div
                    variants={lineGrow}
                    initial="hidden"
                    whileInView="show"
                    viewport={{ once: true }}
                    className="hidden md:block w-20 h-px bg-border origin-left"
                  />
                )}
              </div>
            ))}
          </div>
          <div className="flex md:hidden flex-col items-center mt-2 gap-0">
            {[0, 1].map((i) => (
              <motion.div
                key={i}
                variants={lineGrow}
                initial="hidden"
                whileInView="show"
                viewport={{ once: true }}
                className="w-px h-8 bg-border origin-top"
              />
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
