"use client";

import { signIn } from "next-auth/react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "react-toastify";

export default function LoginForm() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const router = useRouter();

  async function login(e: React.FormEvent) {
    e.preventDefault();
    const signInData = await signIn("credentials", {
      username,
      password,
      redirect: false,
    });
    if (signInData?.error) {
      toast.error("Invalid Username or Password");
    } else {
      router.refresh();
      toast.success("Welcome back, " + username);
    }
  }

  return (
    <div className="weicms-card weicms-auth-widget">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-right-to-bracket text-gold" />
        </div>
        <h3 className="header-title">Account Login</h3>
        <Link
          href="/register"
          className="text-muted"
          title="Create a New Account"
        >
          <i className="fa-solid fa-user-plus" />
        </Link>
      </div>
      <div className="weicms-card-body">
        <form onSubmit={login} className="weicms-form">
          <div className="mb-3">
            <label
              className="block text-xs uppercase text-muted mb-1 font-semibold"
              htmlFor="sidebarLoginUser"
            >
              Account Name
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-user input-icon" />
              <input
                type="text"
                id="sidebarLoginUser"
                className="weicms-input"
                placeholder="Enter username..."
                minLength={3}
                required
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
            </div>
          </div>

          <div className="mb-4">
            <label
              className="block text-xs uppercase text-muted mb-1 font-semibold"
              htmlFor="sidebarLoginPwd"
            >
              Password
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-lock input-icon" />
              <input
                type="password"
                id="sidebarLoginPwd"
                className="weicms-input"
                placeholder="Enter password..."
                minLength={3}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
          </div>

          <button
            type="submit"
            className="btn weicms-btn-primary btn-block mb-3"
          >
            <i className="fa-solid fa-arrow-right-to-bracket" /> Sign In
          </button>

          <div className="text-center text-xs">
            <span className="text-muted">Don&apos;t have an account?</span>
            <Link href="/register" className="text-gold font-bold ml-1">
              Create Account
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}
