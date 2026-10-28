import { getServerSession } from "next-auth";
import LoginForm from "./LoginForm";
import { authOptions } from "@/lib/auth";
import UserPanel from "./UserPanel";

export default async function Section1() {
  const session = await getServerSession(authOptions);
  const isLogged = session?.user != undefined;

  return (
    <>
      {!isLogged && <LoginForm />}
      {isLogged && (
        <UserPanel
          role={session.user.role}
          username={session.user.username}
        />
      )}
    </>
  );
}
