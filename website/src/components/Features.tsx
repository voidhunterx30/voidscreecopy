import { motion } from "framer-motion"
import { Monitor, Camera, Tv, Radio, Wifi, RefreshCw, Palette, Shield } from "lucide-react"

const features = [
  {
    icon: Monitor,
    title: "Android Screen Mirroring",
    description: "Mirror your Android screen to Windows using USB or Wi-Fi.",
  },
  {
    icon: Camera,
    title: "Camera Sharing",
    description: "Use your Android camera as part of your Windows workflow.",
  },
  {
    icon: Tv,
    title: "TV Casting",
    description: "Cast Android content to supported TVs over Wi-Fi.",
  },
  {
    icon: Radio,
    title: "OBS Integration",
    description: "Bring screen and camera feeds into OBS for streaming.",
  },
  {
    icon: Wifi,
    title: "USB + Wi-Fi",
    description: "Connect using USB for reliability or Wi-Fi for convenience.",
  },
  {
    icon: RefreshCw,
    title: "Automatic Updates",
    description: "Stay updated with the built-in automatic update system.",
  },
  {
    icon: Palette,
    title: "Dark / Light Mode",
    description: "Switch between dark and light appearance modes.",
  },
  {
    icon: Shield,
    title: "Authentication",
    description: "User accounts and admin controls for access management.",
  },
]

export default function Features() {
  return (
    <section id="features" className="py-32 max-w-7xl mx-auto px-6">
      <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
        Everything you need to connect your devices.
      </h2>
      <p className="text-text-secondary text-center mb-16">
        Built for real workflows.
      </p>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {features.map((feature, i) => {
          const Icon = feature.icon
          return (
            <motion.div
              key={feature.title}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: i * 0.05 }}
              className="bg-bg-card border border-border rounded-xl p-6 hover:border-border-strong transition-all duration-200"
            >
              <div className="w-10 h-10 rounded-lg bg-accent-subtle flex items-center justify-center text-accent">
                <Icon size={20} />
              </div>
              <h3 className="text-base font-semibold mt-4">{feature.title}</h3>
              <p className="text-sm text-text-secondary mt-2">
                {feature.description}
              </p>
            </motion.div>
          )
        })}
      </div>
    </section>
  )
}
