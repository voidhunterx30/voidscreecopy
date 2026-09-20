import { motion } from "framer-motion";
import { Download, Usb, Settings, Play } from "lucide-react";

const steps = [
  {
    num: "01",
    icon: Download,
    title: "Install",
    description: "Download and install voidscreecopy on your Windows PC.",
  },
  {
    num: "02",
    icon: Usb,
    title: "Connect",
    description: "Connect your Android device using USB or Wi-Fi.",
  },
  {
    num: "03",
    icon: Settings,
    title: "Choose",
    description:
      "Select screen mirroring, camera sharing, TV casting, or OBS mode.",
  },
  {
    num: "04",
    icon: Play,
    title: "Create",
    description:
      "Share your Android screen or camera in your Windows workflow.",
  },
];

const container = {
  hidden: {},
  show: {
    transition: {
      staggerChildren: 0.15,
    },
  },
};

const item = {
  hidden: { opacity: 0, x: -20 },
  show: { opacity: 1, x: 0, transition: { duration: 0.5 } },
};

export default function HowItWorks() {
  return (
    <section id="how-it-works" className="py-32">
      <div className="max-w-5xl mx-auto px-6">
        <h2 className="text-4xl md:text-5xl font-bold text-center mb-20">
          How voidscreecopy works
        </h2>

        <motion.div
          className="relative space-y-16"
          variants={container}
          initial="hidden"
          whileInView="show"
          viewport={{ once: true, margin: "-100px" }}
        >
          <div className="absolute left-6 top-12 bottom-12 w-px bg-border" />

          {steps.map((step) => (
            <motion.div
              key={step.num}
              variants={item}
              className="relative flex gap-6 items-start"
            >
              <div className="relative z-10 w-12 h-12 rounded-full bg-accent text-white flex items-center justify-center flex-shrink-0 font-semibold text-sm">
                {step.num}
              </div>

              <div className="pt-2">
                <h3 className="text-xl font-semibold">{step.title}</h3>
                <p className="text-text-secondary mt-1">{step.description}</p>
              </div>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
