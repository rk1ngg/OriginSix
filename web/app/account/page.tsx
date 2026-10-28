"use client";

import { getSession } from "next-auth/react";
import { useState } from "react";
import { toast } from "react-toastify";

export default function Account() {
  const [oldPassword, setOldPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [repeatNewPassword, setRepeatNewPassword] = useState("");

  async function changePassword(e: React.FormEvent) {
    e.preventDefault();
    const session = await getSession();
    const loginName = session?.user?.username;
    if (!loginName) {
      toast.error("Please sign in first.");
      return;
    }

    const result = await fetch(`/api/account/changepassword`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name: loginName,
        oldPassword,
        newPassword,
        repeatNewPassword,
      }),
    });
    const body = await result.json();
    if (result.ok) {
      toast.success(body.message);
      setOldPassword("");
      setNewPassword("");
      setRepeatNewPassword("");
    } else {
      toast.error(body.message);
    }
  }

  return (
    <div className="weicms-card">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-user-gear text-gold" />
        </div>
        <h3 className="header-title">Account Settings — Change Password</h3>
      </div>

      <div className="weicms-card-body">
        <form onSubmit={changePassword} className="max-w-md mx-auto py-4">
          <div className="mb-4">
            <label className="block text-xs uppercase text-muted mb-1 font-semibold">
              Current Password
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-lock input-icon" />
              <input
                onChange={(e) => setOldPassword(e.target.value)}
                value={oldPassword}
                className="weicms-input"
                type="password"
                placeholder="Enter current password..."
                minLength={4}
                required
              />
            </div>
          </div>

          <div className="mb-4">
            <label className="block text-xs uppercase text-muted mb-1 font-semibold">
              New Password
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-key input-icon" />
              <input
                onChange={(e) => setNewPassword(e.target.value)}
                value={newPassword}
                className="weicms-input"
                type="password"
                placeholder="Minimum 8 characters..."
                minLength={8}
                required
              />
            </div>
          </div>

          <div className="mb-6">
            <label className="block text-xs uppercase text-muted mb-1 font-semibold">
              Confirm New Password
            </label>
            <div className="input-with-icon">
              <i className="fa-solid fa-shield-halved input-icon" />
              <input
                onChange={(e) => setRepeatNewPassword(e.target.value)}
                value={repeatNewPassword}
                className="weicms-input"
                type="password"
                placeholder="Repeat new password..."
                minLength={8}
                required
              />
            </div>
          </div>

          <button type="submit" className="btn weicms-btn-primary btn-block">
            <i className="fa-solid fa-floppy-disk" /> Update Password
          </button>
        </form>
      </div>
    </div>
  );
}
