import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { Download, Menu, X, Home } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigateToSection, useNavigateHome } from "../lib/navigation";

const NAVBAR_HEIGHT = 80;

export default function Navbar() {
  const [scrolled, setScrolled] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const navigateToSection = useNavigateToSection();
  const navigateHome = useNavigateHome();

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 20);
    window.addEventListener("scroll", onScroll);
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  useEffect(() => {
    if (window.location.hash) {
      const id = window.location.hash.replace("#", "");
      let attempts = 0;
      const interval = setInterval(() => {
        attempts++;
        const el = document.getElementById(id);
        if (el || attempts > 60) {
          clearInterval(interval);
          if (el) {
            const top = el.getBoundingClientRect().top + window.scrollY - NAVBAR_HEIGHT;
            window.scrollTo({ top, behavior: "smooth" });
          }
        }
      }, 50);
    }
  }, []);

  const sectionLinks = [
    { label: "Product", id: "product" },
    { label: "Features", id: "features" },
    { label: "How It Works", id: "how-it-works" },
    { label: "OBS", id: "obs" },
  ];

  const handleSectionClick = (id: string) => {
    setMobileOpen(false);
    navigateToSection(id);
  };

  const handleHomeClick = () => {
    setMobileOpen(false);
    navigateHome();
  };

  return (
    <nav
      className={`fixed top-0 left-0 right-0 z-50 transition-colors duration-200 ${
        scrolled
          ? "bg-bg/80 backdrop-blur-xl border-b border-border"
          : "bg-transparent"
      }`}
    >
      <div className="max-w-6xl mx-auto px-6 h-16 flex items-center justify-between">
        <button
          onClick={handleHomeClick}
          className="font-bold text-lg text-text-primary cursor-pointer bg-transparent border-none"
          style={{ fontFamily: "Inter" }}
        >
          voidscreecopy
        </button>

        <div className="hidden md:flex items-center gap-8">
          <button
            onClick={handleHomeClick}
            className="text-sm text-text-secondary hover:text-text-primary transition-colors cursor-pointer bg-transparent border-none"
          >
            <Home size={16} />
          </button>
          {sectionLinks.map((link) => (
            <button
              key={link.id}
              onClick={() => handleSectionClick(link.id)}
              className="text-sm text-text-secondary hover:text-text-primary transition-colors cursor-pointer bg-transparent border-none"
            >
              {link.label}
            </button>
          ))}
        </div>

        <div className="hidden md:block">
          <Link
            to="/download"
            className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-accent text-sm font-medium text-white hover:opacity-90 transition-opacity"
          >
            <Download size={16} />
            Download for Windows
          </Link>
        </div>

        <button
          className="md:hidden text-text-primary bg-transparent border-none cursor-pointer"
          onClick={() => setMobileOpen(!mobileOpen)}
        >
          {mobileOpen ? <X size={22} /> : <Menu size={22} />}
        </button>
      </div>

      <AnimatePresence>
        {mobileOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: "auto" }}
            exit={{ opacity: 0, height: 0 }}
            className="md:hidden bg-bg/95 backdrop-blur-xl border-b border-border overflow-hidden"
          >
            <div className="px-6 py-4 flex flex-col gap-4">
              <button
                onClick={handleHomeClick}
                className="text-sm text-text-secondary hover:text-text-primary transition-colors text-left cursor-pointer bg-transparent border-none"
              >
                Home
              </button>
              {sectionLinks.map((link) => (
                <button
                  key={link.id}
                  onClick={() => handleSectionClick(link.id)}
                  className="text-sm text-text-secondary hover:text-text-primary transition-colors text-left cursor-pointer bg-transparent border-none"
                >
                  {link.label}
                </button>
              ))}
              <Link
                to="/download"
                onClick={() => setMobileOpen(false)}
                className="inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg bg-accent text-sm font-medium text-white"
              >
                <Download size={16} />
                Download for Windows
              </Link>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </nav>
  );
}
