"use client";

import Link from "next/link";
import { useState } from "react";
import { toast } from "react-toastify";

export default function Register() {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [repeatPassword, setRepeatPassword] = useState("");
  const [accepted, setAccepted] = useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password !== repeatPassword) {
      toast.error("The passwords must coincide");
      return;
    }
    const response = await fetch(`/api/account/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        LoginName: username,
        EMail: email,
        Password: password,
        RepeatPassword: repeatPassword,
      }),
    }).then((resp) => resp.json());

    if (response.user === null) {
      toast.error(response.message);
    } else {
      toast.success(response.message);
      setEmail("");
      setUsername("");
      setPassword("");
      setRepeatPassword("");
    }
  };

  return (
    <div className="weicms-card">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-user-plus text-gold" />
        </div>
        <h3 className="header-title">Create New Game Account</h3>
      </div>

      <div className="weicms-card-body">
        <form onSubmit={onSubmit} className="max-w-md mx-auto py-4">
          <div className="mb-4">
            <label className="block text-xs uppercase text-muted mb-1 font-semibold">
              Account Username
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-user input-icon" />
              <input
                type="text"
                placeholder="4–10 characters..."
                minLength={4}
                maxLength={10}
                required
                onChange={(e) => setUsername(e.target.value)}
                value={username}
                className="weicms-input"
              />
            </div>
          </div>

          <div className="mb-4">
            <label className="block text-xs uppercase text-muted mb-1 font-semibold">
              Email Address
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-envelope input-icon" />
              <input
                type="email"
                placeholder="your@email.com"
                minLength={6}
                required
                onChange={(e) => setEmail(e.target.value)}
                value={email}
                className="weicms-input"
              />
            </div>
          </div>

          <div className="mb-4">
            <label className="block text-xs uppercase text-muted mb-1 font-semibold">
              Password
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-lock input-icon" />
              <input
                type="password"
                placeholder="Minimum 8 characters..."
                minLength={8}
                required
                onChange={(e) => setPassword(e.target.value)}
                value={password}
                className="weicms-input"
              />
            </div>
          </div>

          <div className="mb-5">
            <label className="block text-xs uppercase text-muted mb-1 font-semibold">
              Confirm Password
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-shield-halved input-icon" />
              <input
                type="password"
                placeholder="Repeat password..."
                minLength={8}
                required
                onChange={(e) => setRepeatPassword(e.target.value)}
                value={repeatPassword}
                className="weicms-input"
              />
            </div>
          </div>

          <label className="flex items-center gap-2 text-xs text-muted mb-6 cursor-pointer">
            <input
              type="checkbox"
              checked={accepted}
              onChange={(e) => setAccepted(e.target.checked)}
              className="w-4 h-4 accent-amber-500"
            />
            <span>
              I have read and accept the{" "}
              <Link
                href="/terms-and-conditions"
                className="text-gold font-semibold"
              >
                Terms of Service &amp; Server Rules
              </Link>
            </span>
          </label>

          <button
            type="submit"
            disabled={!accepted}
            className="btn weicms-btn-primary btn-block"
          >
            <i className="fa-solid fa-user-check" /> Complete Registration
          </button>
        </form>
      </div>
    </div>
  );
}
