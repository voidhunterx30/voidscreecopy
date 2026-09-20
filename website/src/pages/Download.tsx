import { motion } from "framer-motion";
import { Download as DownloadIcon, Copy, Check, Shield, FileCheck, Terminal } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { appRelease } from "../config/release";

export default function Download() {
  const [copied, setCopied] = useState(false);

  const copyHash = () => {
    navigator.clipboard.writeText(appRelease.sha256);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <main className="min-h-screen pt-28 pb-20">
      <div className="max-w-5xl mx-auto px-6">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center gap-2 bg-accent-subtle text-accent text-xs font-semibold tracking-wider uppercase px-4 py-2 rounded-full mb-6">
            <DownloadIcon size={14} />
            <span>Latest Release</span>
          </div>
          <h1 className="text-4xl md:text-6xl font-bold mb-4">
            Download voidscreecopy
          </h1>
          <p className="text-text-secondary text-lg max-w-xl mx-auto">
            Bring your Android screen and camera into your Windows workflow.
          </p>
        </motion.div>

        {/* Main Download Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.1 }}
          className="bg-bg-card border border-border rounded-2xl p-8 md:p-12 mb-8"
        >
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-8">
            {/* Left: Info */}
            <div className="flex-1">
              <div className="flex items-center gap-3 mb-6">
                <div className="w-14 h-14 rounded-xl bg-accent-subtle flex items-center justify-center">
                  <DownloadIcon size={24} className="text-accent" />
                </div>
                <div>
                  <h2 className="text-2xl font-bold">voidscreecopy</h2>
                  <p className="text-text-secondary text-sm">
                    v{appRelease.version}
                  </p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4 mb-6">
                <div>
                  <p className="text-xs text-text-muted uppercase tracking-wider mb-1">
                    Platform
                  </p>
                  <p className="text-sm font-medium">{appRelease.platform}</p>
                </div>
                <div>
                  <p className="text-xs text-text-muted uppercase tracking-wider mb-1">
                    Architecture
                  </p>
                  <p className="text-sm font-medium">{appRelease.architecture}</p>
                </div>
                <div>
                  <p className="text-xs text-text-muted uppercase tracking-wider mb-1">
                    Installer Size
                  </p>
                  <p className="text-sm font-medium">{appRelease.installerSize}</p>
                </div>
                <div>
                  <p className="text-xs text-text-muted uppercase tracking-wider mb-1">
                    Release
                  </p>
                  <p className="text-sm font-medium">v{appRelease.version}</p>
                </div>
              </div>

              {/* SHA-256 */}
              <div className="bg-bg rounded-lg border border-border p-4">
                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center gap-2">
                    <Shield size={14} className="text-text-muted" />
                    <span className="text-xs text-text-muted uppercase tracking-wider font-semibold">
                      SHA-256
                    </span>
                  </div>
                  <button
                    onClick={copyHash}
                    className="flex items-center gap-1.5 text-xs text-text-secondary hover:text-accent transition-colors cursor-pointer"
                  >
                    {copied ? (
                      <>
                        <Check size={12} />
                        Copied
                      </>
                    ) : (
                      <>
                        <Copy size={12} />
                        Copy hash
                      </>
                    )}
                  </button>
                </div>
                <code className="text-xs text-text-secondary font-mono break-all leading-relaxed block">
                  {appRelease.sha256}
                </code>
              </div>
            </div>

            {/* Right: Download Button */}
            <div className="md:w-64 flex flex-col items-center gap-4">
              <a
                href={appRelease.downloadUrl}
                className="w-full flex items-center justify-center gap-3 bg-accent hover:bg-accent-hover text-white font-semibold py-4 px-8 rounded-xl transition-colors text-lg no-underline"
              >
                <DownloadIcon size={20} />
                Download for Windows
              </a>
              <p className="text-xs text-text-muted text-center">
                Direct download — no GitHub account required
              </p>
            </div>
          </div>
        </motion.div>

        {/* Installation Guide */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.2 }}
          className="bg-bg-card border border-border rounded-2xl p-8 md:p-12"
        >
          <h3 className="text-xl font-bold mb-6 flex items-center gap-3">
            <Terminal size={20} className="text-accent" />
            Install
          </h3>
          <div className="space-y-4">
            {[
              {
                step: 1,
                title: "Download the installer",
                desc: "Click the Download button above. The installer will begin downloading directly.",
              },
              {
                step: 2,
                title: "Open the installer",
                desc: `Run voidscreecopy-installer.exe from your Downloads folder.`,
              },
              {
                step: 3,
                title: "Follow the wizard",
                desc: "Accept the license agreement, choose your install location, and complete the installation.",
              },
              {
                step: 4,
                title: "Launch voidscreecopy",
                desc: "Open voidscreecopy from your Start Menu or desktop shortcut.",
              },
              {
                step: 5,
                title: "Connect your Android",
                desc: "Connect your Android device via USB cable or Wi-Fi pairing.",
              },
              {
                step: 6,
                title: "Start sharing",
                desc: "Choose Screen Mirroring, Camera Sharing, TV Casting, or OBS mode from the app.",
              },
            ].map((item) => (
              <div
                key={item.step}
                className="flex gap-4 items-start"
              >
                <div className="flex-shrink-0 w-8 h-8 rounded-lg bg-accent-subtle flex items-center justify-center">
                  <span className="text-accent text-sm font-bold">
                    {item.step}
                  </span>
                </div>
                <div>
                  <p className="font-semibold text-sm">{item.title}</p>
                  <p className="text-text-secondary text-sm mt-0.5">
                    {item.desc}
                  </p>
                </div>
              </div>
            ))}
          </div>

          <div className="mt-8 pt-6 border-t border-border flex items-center gap-3">
            <FileCheck size={16} className="text-text-muted" />
            <p className="text-xs text-text-muted">
              This installer is verified and contains everything you need. No additional software required.
            </p>
          </div>
        </motion.div>

        {/* Bottom CTA */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.5, delay: 0.3 }}
          className="text-center mt-12"
        >
          <Link
            to="/"
            className="text-sm text-text-secondary hover:text-accent transition-colors no-underline"
          >
            ← Back to voidscreecopy
          </Link>
        </motion.div>
      </div>
    </main>
  );
}
