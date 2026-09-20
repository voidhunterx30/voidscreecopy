import { Link } from "react-router-dom";
import { appRelease } from "../config/release";
import { useNavigateToSection, useNavigateHome } from "../lib/navigation";

export default function Footer() {
  const navigateToSection = useNavigateToSection();
  const navigateHome = useNavigateHome();

  const sectionLinks = [
    { label: "Product", id: "product" },
    { label: "Features", id: "features" },
    { label: "How It Works", id: "how-it-works" },
    { label: "OBS", id: "obs" },
  ];

  return (
    <footer className="border-t border-border bg-bg">
      <div className="max-w-7xl mx-auto px-6 py-16">
        <div className="md:grid-cols-3 gap-12 grid">
          <div className="md:col-span-1">
            <button
              onClick={navigateHome}
              className="font-bold text-lg text-text-primary cursor-pointer bg-transparent border-none p-0"
            >
              voidscreecopy
            </button>
            <p className="text-sm text-text-secondary mt-3 max-w-xs">
              Low-latency Android screen, camera, and TV sharing for Windows.
            </p>
          </div>

          <div className="md:col-span-1">
            <h4 className="text-sm font-semibold mb-4">Product</h4>
            <ul className="space-y-3 list-none p-0 m-0">
              {sectionLinks.map((link) => (
                <li key={link.id}>
                  <button
                    onClick={() => navigateToSection(link.id)}
                    className="text-sm text-text-secondary hover:text-text-primary transition-colors cursor-pointer bg-transparent border-none p-0"
                  >
                    {link.label}
                  </button>
                </li>
              ))}
              <li>
                <Link
                  to="/download"
                  className="text-sm text-text-secondary hover:text-text-primary transition-colors"
                >
                  Download
                </Link>
              </li>
            </ul>
          </div>

          <div className="md:col-span-1">
            <h4 className="text-sm font-semibold mb-4">Resources</h4>
            <ul className="space-y-3 list-none p-0 m-0">
              <li>
                <a
                  href="https://github.com/voidhunterx30/voidscreecopy"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-text-secondary hover:text-text-primary transition-colors"
                >
                  GitHub
                </a>
              </li>
              <li>
                <a
                  href="https://github.com/voidhunterx30/voidscreecopy/releases"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-text-secondary hover:text-text-primary transition-colors"
                >
                  Releases
                </a>
              </li>
              <li>
                <Link
                  to="/download"
                  className="text-sm text-text-secondary hover:text-text-primary transition-colors"
                >
                  Download
                </Link>
              </li>
            </ul>
          </div>
        </div>

        <div className="border-t border-border mt-12 pt-8 flex flex-col sm:flex-row justify-between items-center gap-4">
          <span className="text-sm text-text-muted">© 2026 voidscreecopy</span>
          <span className="text-sm text-text-muted">v{appRelease.version}</span>
        </div>
      </div>
    </footer>
  );
}
